using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState { Patrolling, Chasing, Searching }

    [Header("Settings")]
    [SerializeField] private EnemyState currentState;
    [SerializeField] private float walkSpeed = 3.5f;       
    [SerializeField] private float runSpeed = 6.0f;        

    [Header("Detection Parameters")]
    [SerializeField] private float detectionRadius = 10f;  
    [SerializeField] private float fieldOfViewAngle = 60f; 
    [SerializeField] private LayerMask playerLayer;        
    [SerializeField] private LayerMask obstacleLayer;      

    [Header("Patrol Parameters")]
    [SerializeField] private Transform[] patrolPoints;     
    [SerializeField] private float waitTimeAtPoint = 2f;   
    [SerializeField] private bool randomizePatrol = true; // Toggle for random pathing

    [Header("Search Parameters")]
    [SerializeField] private float searchDuration = 5f;    

    [Header("Interaction Parameters")]
    [SerializeField] private float doorInteractionRange = 1.5f; // Distance to check for doors
    [SerializeField] private float doorCheckInterval = 0.5f;    // Frequency of door checks

    [Header("Dynamic Difficulty (New)")]
    [SerializeField] private int encounterCount = 0;       // How many times player was seen
    [SerializeField] private float rageMultiplier = 1.1f;  // Stats increase by 10% per encounter
    [SerializeField] private float maxSpeedCap = 9.0f;     // Limit so he doesn't become Sonic

    [Header("References")]
    private NavMeshAgent agent;
    private Transform playerTarget;
    private int currentPatrolIndex = 0;
    private float waitTimer;
    private float searchTimer;
    private float doorCheckTimer;                          // Timer for door checking
    private Vector3 lastKnownPosition;                     

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (PlayerSingleton.instance != null)
        {
            playerTarget = PlayerSingleton.instance.transform;
        }

        // Validate NavMesh placement and snap agent to surface
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas)) 
        {
            agent.Warp(hit.position); 
        }
        else
        {
            Debug.LogError("CRITICAL ERROR: No NavMesh found within 5m.");
            enabled = false; 
            return;
        }

        // Pick a random start point if randomized
        if (randomizePatrol && patrolPoints.Length > 0)
        {
            currentPatrolIndex = Random.Range(0, patrolPoints.Length);
        }

        currentState = EnemyState.Patrolling;
    }

    void Update()
    {
        if (playerTarget == null) return;

        // Check for doors periodically
        doorCheckTimer += Time.deltaTime;
        if(doorCheckTimer >= doorCheckInterval)
        {
            CheckForDoors();
            doorCheckTimer = 0f;
        }

        // Detection Logic
        if (CanSeePlayer())
        {
            lastKnownPosition = playerTarget.position;
            if (currentState != EnemyState.Chasing)
            {
                IncreaseAggression();
            }

            currentState = EnemyState.Chasing;
        }

        switch (currentState)
        {
            case EnemyState.Patrolling:
                PatrolBehavior();
                break;
            case EnemyState.Chasing:
                ChaseBehavior();
                break;
            case EnemyState.Searching:
                SearchBehavior();
                break;
        }
    }

    // Logic for opening doors while ignoring lockers
    private void CheckForDoors()
    {
        // Create a check sphere in front of the enemy
        Vector3 checkPosition = transform.position + transform.forward * 1.0f;
        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, doorInteractionRange);

        foreach (var hit in hitColliders)
        {
            // Only interact with objects tagged "Door", ignore "Locker"
            if (hit.CompareTag("Door"))
            {
                DoorController door = hit.GetComponent<DoorController>();
                
                if (door != null && !door.isOpen)
                {
                    door.PlayAnimation();
                }
            }
        }
    }

    private void IncreaseAggression()
    {
        encounterCount++;
        Debug.Log($"Enemy Enraged! Encounter count: {encounterCount}");

        // Increase speeds but clamp to max limit
        walkSpeed = Mathf.Min(walkSpeed * rageMultiplier, maxSpeedCap);
        runSpeed = Mathf.Min(runSpeed * rageMultiplier, maxSpeedCap);
        
        // Increase senses
        detectionRadius = Mathf.Min(detectionRadius * 1.1f, 20f); // Cap vision at 20m
        searchDuration += 1.0f; 
    }

    private void PatrolBehavior()
    {
        agent.speed = walkSpeed;

        if (patrolPoints.Length == 0) return;

        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                ChooseNextPatrolPoint();
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                waitTimer = 0f;
            }
        }
    }

    private void ChooseNextPatrolPoint()
    {
        if (randomizePatrol)
        {
            int newIndex = currentPatrolIndex;
            if (patrolPoints.Length > 1) 
            {
                while (newIndex == currentPatrolIndex)
                {
                    newIndex = Random.Range(0, patrolPoints.Length);
                }
            }
            currentPatrolIndex = newIndex;
        }
    }

    private void ChaseBehavior()
    {
        agent.speed = runSpeed;
        agent.SetDestination(playerTarget.position);

        if (!CanSeePlayer())
        {
            currentState = EnemyState.Searching;
            agent.SetDestination(lastKnownPosition); 
            searchTimer = 0f;
        }
        
        if (Vector3.Distance(transform.position, playerTarget.position) < 1.5f)
        {
            // Placeholder for attack logic
            Debug.Log("Player caught.");
        }
    }

    private void SearchBehavior()
    {
        agent.speed = walkSpeed;

        if (agent.remainingDistance < 0.5f)
        {
            searchTimer += Time.deltaTime;
            
            if (searchTimer > searchDuration)
            {
                currentState = EnemyState.Patrolling;
                // Go to the nearest patrol point instead of random to look smarter
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (playerTarget == null) return false;

        Vector3 enemyEyes = transform.position + Vector3.up * 1.6f;
        Vector3 playerCenter = playerTarget.position + Vector3.up * 1.6f;

        Vector3 directionToPlayer = (playerCenter - enemyEyes).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer < detectionRadius)
        {
            if (Vector3.Angle(transform.forward, directionToPlayer) < fieldOfViewAngle / 2)
            {
                if (!Physics.Raycast(enemyEyes, directionToPlayer, distanceToPlayer, obstacleLayer))
                {
                    return true; 
                }
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Vector3 viewAngleA = DirFromAngle(-fieldOfViewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(fieldOfViewAngle / 2, false);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * detectionRadius);

        // Visualizes door interaction range
        Gizmos.color = Color.blue;
        Vector3 doorCheckPos = transform.position + transform.forward * 1.0f;
        Gizmos.DrawWireSphere(doorCheckPos, doorInteractionRange);
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}