using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState { Patrolling, Chasing, Searching, InteractingWithDoor }

    [Header("Settings")]
    [SerializeField] private EnemyState currentState;
    [SerializeField] private float walkSpeed = 1.8f;
    [SerializeField] private float runSpeed = 3.0f;
    [SerializeField] private float rotationSpeed = 600f; 

    [Header("Detection Parameters")]
    [SerializeField] private float detectionRadius = 12f;
    [SerializeField] private float nearbySenseRadius = 3.0f; 
    [SerializeField] private float fieldOfViewAngle = 85f; // Recommended: Increase to 85-90 to see up/down stairs
    [SerializeField] private float eyeHeight = 1.6f;          
    [SerializeField] private float playerHeightOffset = 1.0f; 
    [SerializeField] private LayerMask obstacleLayer;      

    [Header("Patrol Parameters")]
    [SerializeField] private Transform[] patrolPoints;     
    [SerializeField] private float waitTimeAtPoint = 2f;   
    [SerializeField] private bool randomizePatrol = true; 

    [Header("Search Parameters")]
    [SerializeField] private float searchDuration = 5f;    

    [Header("Interaction Parameters")]
    [SerializeField] private float doorInteractionRange = 2.5f; 
    [SerializeField] private float doorCheckInterval = 0.2f;    
    [SerializeField] private float doorOpenWaitTime = 1.5f;

    [Header("References")]
    private NavMeshAgent agent;
    private Transform playerTarget;
    private Animator anim; 
    private int currentPatrolIndex = 0;
    private float pathUpdateTimer = 0f;                    
    private float waitTimer;
    private float searchTimer;
    private float doorCheckTimer;                          
    private Vector3 lastKnownPosition;                     
    private bool isHandlingDoor = false;

    // Animation smoothing variables
    private float stopTimer = 0f; 
    private bool isMovingSmoothed = false;

    // --- NEW: STUCK DETECTION VARIABLES ---
    private Vector3 lastStuckCheckPosition;
    private float stuckCheckTimer = 0f;
    private float stuckDuration = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>(); 
        agent.angularSpeed = rotationSpeed;
        
        // Ensure high acceleration to prevent sliding on corners
        agent.acceleration = 40f; 

        if (PlayerSingleton.instance != null)
        {
            playerTarget = PlayerSingleton.instance.transform;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas)) 
        {
            agent.Warp(hit.position); 
        }

        if (randomizePatrol && patrolPoints.Length > 0)
        {
            currentPatrolIndex = Random.Range(0, patrolPoints.Length);
        }

        currentState = EnemyState.Patrolling;
        lastStuckCheckPosition = transform.position;
    }

    void Update()
    {
        // --- ANIMATION LOGIC ---
        if (anim != null) 
        {
            bool currentlyMoving = agent.velocity.magnitude > 0.1f && !agent.isStopped;

            if (currentlyMoving)
            {
                isMovingSmoothed = true;
                stopTimer = 0f; 
            }
            else
            {
                // Delay stopping the animation to prevent flickering on stairs/corners
                stopTimer += Time.deltaTime;
                if (stopTimer > 0.15f)
                {
                    isMovingSmoothed = false;
                }
            }
            
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);

            if (isMovingSmoothed)
            {
                if (currentState == EnemyState.Chasing)
                {
                    anim.SetBool("isRunning", true); 
                }
                else
                {
                    anim.SetBool("isWalking", true); 
                }
            }
        }

        // --- NEW: ANTI-STUCK LOGIC ---
        HandleStuckDetection();

        if (playerTarget == null) return;
        if (currentState == EnemyState.InteractingWithDoor) return;

        doorCheckTimer += Time.deltaTime;
        if(doorCheckTimer >= doorCheckInterval && !isHandlingDoor)
        {
            CheckForDoors();
            doorCheckTimer = 0f;
        }

        // Detection Logic
        if (CanSeePlayer())
        {
            lastKnownPosition = playerTarget.position;
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

    // --- NEW METHOD: Checks if agent is trying to move but hitting a wall ---
    private void HandleStuckDetection()
    {
        if (agent.isStopped || isHandlingDoor) return;

        // Check only if we expect to be moving
        if (agent.velocity.magnitude > 0.1f || currentState == EnemyState.Chasing)
        {
            stuckCheckTimer += Time.deltaTime;
            
            // Check every 0.5 seconds
            if (stuckCheckTimer > 0.5f)
            {
                float distanceMoved = Vector3.Distance(transform.position, lastStuckCheckPosition);
                
                // If moved less than 0.1m in 0.5s while trying to move -> STUCK
                if (distanceMoved < 0.1f)
                {
                    stuckDuration += stuckCheckTimer;
                    
                    // If stuck for more than 1 second -> FORCE RECOVERY
                    if (stuckDuration > 1.0f)
                    {
                        RecoverFromStuck();
                    }
                }
                else
                {
                    // Moving fine, reset counters
                    stuckDuration = 0f;
                }

                lastStuckCheckPosition = transform.position;
                stuckCheckTimer = 0f;
            }
        }
    }

    private void RecoverFromStuck()
    {
        // Debug.Log("Agent stuck! Recovering...");
        stuckDuration = 0f;
        
        // Reset path to force recalculation around the obstacle
        agent.ResetPath();
        
        // If searching or patrolling, maybe skip to next point
        if (currentState == EnemyState.Patrolling)
        {
            ChooseNextPatrolPoint();
        }
    }

    private void CheckForDoors()
    {
        Vector3 checkPosition = transform.position + Vector3.up * 1.0f;
        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, doorInteractionRange);

        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Door"))
            {
                DoorController door = hit.GetComponent<DoorController>();
                
                if (door != null && !door.isOpen && !isHandlingDoor)
                {
                    if (!door.isLocked)
                    {
                        StartCoroutine(OpenDoorSequence(door));
                    }
                }
            }
        }
    }

    private IEnumerator OpenDoorSequence(DoorController door)
    {
        isHandlingDoor = true;
        EnemyState previousState = currentState;
        currentState = EnemyState.InteractingWithDoor;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Force idle animation while opening door
        if(anim != null) 
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
        }

        door.PlayAnimation();

        yield return new WaitForSeconds(doorOpenWaitTime);

        agent.isStopped = false;
        currentState = previousState;
        
        yield return new WaitForSeconds(1.0f);
        isHandlingDoor = false;
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
        else if (patrolPoints.Length > 0)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void ChaseBehavior()
    {
        agent.speed = runSpeed;

        if (agent.isOnOffMeshLink) return;

        pathUpdateTimer += Time.deltaTime;
        
        if (pathUpdateTimer > 0.1f) // Faster updates for smoother chasing
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
                if(patrolPoints.Length > 0)
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (playerTarget == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // Proximity Sense check
        if (distanceToPlayer < nearbySenseRadius)
        {
            return true;
        }

        Vector3 enemyEyes = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPoint = playerTarget.position + Vector3.up * playerHeightOffset; 

        Vector3 directionToPlayer = (targetPoint - enemyEyes).normalized;

        if (distanceToPlayer < detectionRadius)
        {
            // Modified: Using 3D Angle instead of flat angle to detect player on stairs (elevation changes)
            if (Vector3.Angle(transform.forward, directionToPlayer) < fieldOfViewAngle / 2)
            {
                RaycastHit hit;
                if (!Physics.Raycast(enemyEyes, directionToPlayer, out hit, distanceToPlayer, obstacleLayer))
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
        Gizmos.DrawWireSphere(transform.position, nearbySenseRadius);

        Gizmos.color = Color.blue;
        Vector3 doorCheckPos = transform.position + Vector3.up * 1.0f;
        Gizmos.DrawWireSphere(doorCheckPos, doorInteractionRange);
        
        // Visualize approximate vision direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, transform.forward * detectionRadius);
    }
}