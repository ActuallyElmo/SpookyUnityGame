using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState { Patrolling, Chasing, Searching }

    [Header("Settings")]
    [SerializeField] private EnemyState currentState;
    [SerializeField] private float walkSpeed = 3.5f;       // Base walking speed
    [SerializeField] private float runSpeed = 6.0f;        // Chasing speed

    [Header("Detection Parameters")]
    [SerializeField] private float detectionRadius = 10f;  // Vision range
    [SerializeField] private float fieldOfViewAngle = 60f; // View angle cone
    [SerializeField] private LayerMask playerLayer;        // Layer for player detection
    [SerializeField] private LayerMask obstacleLayer;      // Layer for blocking vision

    [Header("Patrol Parameters")]
    [SerializeField] private Transform[] patrolPoints;     // List of patrol waypoints
    [SerializeField] private float waitTimeAtPoint = 2f;   // Time to wait at each point

    [Header("Search Parameters")]
    [SerializeField] private float searchDuration = 5f;    // Time spent searching before returning to patrol

    [Header("References")]
    private NavMeshAgent agent;
    private Transform playerTarget;
    private int currentPatrolIndex = 0;
    private float waitTimer;
    private float searchTimer;
    private Vector3 lastKnownPosition;                     // Stores player's last valid position

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Locates the player instance via Singleton
        if (PlayerSingleton.instance != null)
        {
            playerTarget = PlayerSingleton.instance.transform;
        }

        // Validate NavMesh placement and snap agent to surface
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas)) 
        {
            agent.Warp(hit.position); 
            Debug.Log("Enemy agent successfully snapped to NavMesh.");
        }
        else
        {
            Debug.LogError("CRITICAL ERROR: No NavMesh found within 5m. Agent is too far from the baked surface.");
            enabled = false; 
            return;
        }

        currentState = EnemyState.Patrolling;
    }

    void Update()
    {
        if (playerTarget == null) return;

        // Handles state transitions based on detection
        if (CanSeePlayer())
        {
            lastKnownPosition = playerTarget.position;
            currentState = EnemyState.Chasing;
        }

        // Executes logic based on current state
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

    private void PatrolBehavior()
    {
        agent.speed = walkSpeed;

        if (patrolPoints.Length == 0) return;

        // Checks if destination is reached
        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                // Cycle to the next patrol point
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                waitTimer = 0f;
            }
        }
    }

    private void ChaseBehavior()
    {
        agent.speed = runSpeed;
        agent.SetDestination(playerTarget.position);

        // Switch to search if visual contact is lost
        if (!CanSeePlayer())
        {
            currentState = EnemyState.Searching;
            agent.SetDestination(lastKnownPosition); 
            searchTimer = 0f;
        }
        
        // Interaction logic when close to player
        if (Vector3.Distance(transform.position, playerTarget.position) < 1.5f)
        {
            Debug.Log("Player caught.");
        }
    }

    private void SearchBehavior()
    {
        agent.speed = walkSpeed;

        // Wait at last known position
        if (agent.remainingDistance < 0.5f)
        {
            searchTimer += Time.deltaTime;
            
            // Return to patrol after search duration expires
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

        // 1. Get player's current height and determine the target point.
        CharacterController playerCC = playerTarget.GetComponent<CharacterController>();
        
        // Define player target point based on their current height
        float playerCenterOffset = 1.6f; // Default standing height offset (e.g., center of a 3.2m character)
        if (playerCC != null)
        {
            // Use the CharacterController's current height to find the center
            // The center of a standing or crouching CC is typically at half its height.
            playerCenterOffset = playerCC.height * 0.4f; 
        }
        
        Vector3 playerTargetPoint = playerTarget.position + Vector3.up * playerCenterOffset;

        // 2. Define the Enemy's eye height
        float enemyEyeHeight = 1.6f; 
        Vector3 enemyEyes = transform.position + Vector3.up * enemyEyeHeight;

        // 3. Calculate direction and distance for Raycast
        Vector3 directionToPlayer = (playerTargetPoint - enemyEyes).normalized;
        // Calculate distance from eye to target point
        float distanceToPlayer = Vector3.Distance(enemyEyes, playerTargetPoint); 

        // Check distance within radius (using base-to-base distance for the sphere check)
        if (Vector3.Distance(transform.position, playerTarget.position) < detectionRadius)
        {
            // Check if target is within field of view (using flat, horizontal angle)
            Vector3 enemyForwardFlat = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
            Vector3 directionFlat = new Vector3(directionToPlayer.x, 0, directionToPlayer.z).normalized;

            if (Vector3.Angle(enemyForwardFlat, directionFlat) < fieldOfViewAngle / 2)
            {
                // Verify line of sight using Raycast
                // Cast from the enemy's eye position towards the player's dynamic center point.
                if (!Physics.Raycast(enemyEyes, directionToPlayer, distanceToPlayer, obstacleLayer))
                {
                    // Player is visible through the line of sight check
                    return true; 
                }
            }
        }
        return false;
    }

    // Visualizes detection range and angle in Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Vector3 viewAngleA = DirFromAngle(-fieldOfViewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(fieldOfViewAngle / 2, false);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * detectionRadius);
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}