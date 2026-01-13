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
    [SerializeField] private float eyeHeight = 1.5f;          // Height of the enemy's eyes
    [SerializeField] private float playerHeightOffset = 1.0f; // Target height offset on the player (e.g., chest level)
    [SerializeField] private LayerMask playerLayer;        
    [SerializeField] private LayerMask obstacleLayer;      

    [Header("Patrol Parameters")]
    [SerializeField] private Transform[] patrolPoints;     
    [SerializeField] private float waitTimeAtPoint = 2f;   
    [SerializeField] private bool randomizePatrol = true; 

    [Header("Search Parameters")]
    [SerializeField] private float searchDuration = 5f;    

    [Header("Interaction Parameters")]
    [SerializeField] private float doorInteractionRange = 1.5f; 
    [SerializeField] private float doorCheckInterval = 0.5f;    

    [Header("Dynamic Difficulty")]
    [SerializeField] private int encounterCount = 0;       
    [SerializeField] private float rageMultiplier = 1.1f;  
    [SerializeField] private float maxSpeedCap = 9.0f;     

    [Header("References")]
    private NavMeshAgent agent;
    private Transform playerTarget;
    private int currentPatrolIndex = 0;
    private float pathUpdateTimer = 0f;                    // Timer for path update optimization
    private float waitTimer;
    private float searchTimer;
    private float doorCheckTimer;                          
    private Vector3 lastKnownPosition;                     

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (PlayerSingleton.instance != null)
        {
            playerTarget = PlayerSingleton.instance.transform;
        }

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

        if (randomizePatrol && patrolPoints.Length > 0)
        {
            currentPatrolIndex = Random.Range(0, patrolPoints.Length);
        }

        currentState = EnemyState.Patrolling;
    }

    void Update()
    {
        if (playerTarget == null) return;

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

    private void CheckForDoors()
    {
        Vector3 checkPosition = transform.position + transform.forward * 1.0f;
        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, doorInteractionRange);

        foreach (var hit in hitColliders)
        {
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
        // Debug.Log($"Enemy Enraged! Encounter count: {encounterCount}");

        walkSpeed = Mathf.Min(walkSpeed * rageMultiplier, maxSpeedCap);
        runSpeed = Mathf.Min(runSpeed * rageMultiplier, maxSpeedCap);
        
        detectionRadius = Mathf.Min(detectionRadius * 1.1f, 20f); 
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

        // Prevent path updates while traversing OffMeshLinks
        if (agent.isOnOffMeshLink)
        {
            return;
        }

        // Limit path updates to prevent stuttering on NavMeshLinks
        pathUpdateTimer += Time.deltaTime;
        
        if (pathUpdateTimer > 0.2f) 
        {
            agent.SetDestination(playerTarget.position);
            pathUpdateTimer = 0f;
        }

        if (!CanSeePlayer())
        {
            currentState = EnemyState.Searching;
            agent.SetDestination(lastKnownPosition); 
            searchTimer = 0f;
        }
        
        if (Vector3.Distance(transform.position, playerTarget.position) < 1.5f)
        {
            // Debug.Log("Player caught.");
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
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (playerTarget == null) return false;

        Vector3 enemyEyes = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPoint = playerTarget.position + Vector3.up * playerHeightOffset; 

        Vector3 directionToPlayer = (targetPoint - enemyEyes).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer < detectionRadius)
        {
            // Calculate angle on the horizontal plane (ignoring height)
            Vector3 flatForward = transform.forward; flatForward.y = 0;
            Vector3 flatDir = directionToPlayer; flatDir.y = 0;

            if (Vector3.Angle(flatForward, flatDir) < fieldOfViewAngle / 2)
            {
                RaycastHit hit;
                
                // Perform 3D raycast for visibility check
                if (Physics.Raycast(enemyEyes, directionToPlayer, out hit, distanceToPlayer, obstacleLayer, QueryTriggerInteraction.Ignore))
                {
                    // Visual debug for blocked view
                    Debug.DrawLine(enemyEyes, hit.point, Color.red);
                    return false; 
                }
                
                // Visual debug for clear line of sight
                Debug.DrawLine(enemyEyes, targetPoint, Color.green); 
                return true; 
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // FOV Cone visualization
        Gizmos.color = Color.red;
        Vector3 viewAngleA = DirFromAngle(-fieldOfViewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(fieldOfViewAngle / 2, false);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * detectionRadius);

        Gizmos.color = Color.blue;
        Vector3 doorCheckPos = transform.position + transform.forward * 1.0f;
        Gizmos.DrawWireSphere(doorCheckPos, doorInteractionRange);
        
        // Visualize eye height
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * eyeHeight, 0.1f);

        // Visualize target position on player
        if (playerTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(playerTarget.position + Vector3.up * playerHeightOffset, 0.1f);
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}