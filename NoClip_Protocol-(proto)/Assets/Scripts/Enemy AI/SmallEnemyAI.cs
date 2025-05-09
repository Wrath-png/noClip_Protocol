using System.Collections;
using UnityEngine;
using UnityEngine.AI;

//Will contain the AI for the easy enemies. They will travel from waypoint to waypoint but stop and shoot at the player if they see them.
public class SmallEnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float sightRange, attackRange;
    public float speed;
    public float waitTime = 2f;
    public float timeBetweenAttacks = 1f;
    public bool wandering = false;
    public float wanderRadius = 45f;
    public float wanderTime = 5f;
    public float damageMultiplier = 1f;
    public float memory = 2f;
    public bool enableAttack = true;
    public bool enableWander = false;
    public bool enablePatrol = true;
    public bool enableChase = true;

    [Header("References")]
    
    public Transform[] waypoints;
    public Transform head;
    public LayerMask whatIsWall, whatIsPlayer;
    public AnimationStateChanger animationStateChanger;
    public EnemyAttack attack;

    public NavMeshAgent agent { get; private set; }
    public Transform player { get; private set; }
    public AudioSource smallEnemySound;
    public AudioClip drone;
    public AudioClip explode;
    private EnemyState currentState;
    private int currentWaypointIndex;
    private float waitCounter = 0f;
    public bool waiting = false;
    public bool hasWaited = false;
    private float lastSeenTime = Mathf.NegativeInfinity;
    private bool attacking;

    [Header("Animation")]
    public string idleState = "idle";
    public string lookAroundState = "LookAround";

    //Wandering
    private Vector3 wanderPos;
    private bool wanderPointSet = false;

    void Awake() 
    {
        agent = GetComponent<NavMeshAgent>();
        head = transform.Find("ProtoEnemy");
        player = GameObject.Find("RagequitPlayer").transform;
        attack = GetComponent<EnemyAttack>();
        smallEnemySound = GetComponent<AudioSource>();
        smallEnemySound.clip = drone;
        smallEnemySound.loop = true;
        smallEnemySound.Play();
    }

    public void SetPatrolPath(Transform[] newPath) {
        waypoints = newPath;
        currentWaypointIndex = 0;
    }
    public void SetStats(
        float speed,
        float damageMultiplier,
        float sightRange,
        float attackRange,
        float wanderRadius,
        float memory
        ) {

        this.sightRange = sightRange;
        this.attackRange = attackRange;
        this.wanderRadius = wanderRadius;
        this.speed = speed;
        this.damageMultiplier = damageMultiplier;
        this.memory = memory;
        agent.speed = speed;
    }
    public void SetBehaviors(
        bool attackAllowed, 
        bool wanderAllowed, 
        bool patrolAllowed, 
        bool chaseAllowed
        ) {
        
        enableAttack = attackAllowed;
        enableWander = wanderAllowed;
        enablePatrol = patrolAllowed;
        enableChase = chaseAllowed;
    }

    void Start()
    {
        animationStateChanger = GetComponent<AnimationStateChanger>();
        if (enableWander) {
            TransitionToState(new WanderState(this));  // Start in WanderState
        } else {
            TransitionToState(new PatrolState(this));  // Start in PatrolState
        }
    }
    void Update()
    {
        currentState?.Update();
    }

    public void TransitionToState(EnemyState newState) {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
    public bool CanSeePlayer() {
        Vector3 direction = (player.position - head.position).normalized;
        float angle = Vector3.Angle(head.forward, direction);  //Finds angle between enemy's forward and direction of player
        
        if (angle < 90 && Physics.CheckSphere(transform.position, sightRange, whatIsPlayer)) {

            float distance = Vector3.Distance(head.position, player.position);
            if (!Physics.Raycast(head.position, direction, distance, whatIsWall)) {
                if (Physics.Raycast(head.position, direction, distance, whatIsPlayer)) {
                    lastSeenTime = Time.time;   //Update last seen time
                    ChangeAnimation(idleState);     //Ends look around animation 
                    return true;
                }
            }
        }
        return false;   //Player not in field of vision.
    }
    public bool RememberLocation() {
        return Time.time - lastSeenTime <= memory;  //Returns true if less time than memory has passed
    }
    public bool InAttackRange() {
        // First check if player is within the attack radius
        bool inRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
    
        if (!inRange)
        {
            return false;
        }

        // Now confirm if there's a clear line of sight
        Vector3 direction = (player.position - head.position).normalized;
        float distance = Vector3.Distance(head.position, player.position);

        // Ensure there are no walls between enemy and player
        if (!Physics.Raycast(head.position, direction, distance, whatIsWall))
        {
            return true;
        }

        return false;
    }
    public Vector3 PlayerPositionFlat() {
        return new Vector3(player.position.x, 0f, player.position.z);
    }
    public Vector3 FlatPosition(Vector3 pos) {
        return new Vector3(pos.x, 0f, pos.z);
    }
    public Vector3 PlayerPosition() {
        return new Vector3(player.position.x, player.position.y, player.position.z);
    }
    public void FacePlayer() {
        transform.LookAt(PlayerPosition());
    }
    public float GetDamageMultiplier() {
        return damageMultiplier;
    }
    public void TryAttack() {
        if (!attacking) {
            attacking = true;
            attack.Attack();
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }    
    }
    public void SetNextWaypoint() {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }
    public Vector3 CurrentWaypointPosition() {
        Transform wp = waypoints[currentWaypointIndex];
        return new Vector3(wp.position.x, 0f, wp.position.z);
    }
    public bool AtWaypoint() {
        Transform wp = waypoints[currentWaypointIndex];
        Vector3 flatPosition = FlatPosition(transform.position);
        Vector3 flatWaypoint = FlatPosition(wp.position);
        
        if (!hasWaited && Vector3.Distance(flatPosition, flatWaypoint) < 0.01f) {
            //Debug.Log("Going to Wait");
            ChangeAnimation(lookAroundState);
            waiting = true;
            hasWaited = true;
            StartCoroutine(ResetWaitedFlag());
            return true;
        }
        return false;
    }
    public void WaitAtPoint(){
        if(waiting) {
            waitCounter += Time.deltaTime;
            if(waitCounter < waitTime) {
                return;
            }
            else {
                waiting = false;
                ChangeAnimation(idleState);
                waitCounter = 0f;

                if (enablePatrol && !(currentState is WanderState)) {
                    SetNextWaypoint();
                }
            }
        }
    }
    
    public void ChangeAnimation (string newAnimationState) {
        animationStateChanger.ChangeAnimationState(newAnimationState);
    }
    private void ResetAttack() {
        attacking = false;
    }
    private IEnumerator ResetWaitedFlag() {
        // Wait for some time before resetting the flag
        yield return new WaitForSeconds(3f); // Adjust the delay as needed
        hasWaited = false;
    }
    public void Wander() {
        if(!wanderPointSet)
        {
            SearchWanderPoint();
        }

        //Start moving
        if (wanderPointSet && !wandering) {
            wandering = true;
            agent.SetDestination(wanderPos);
        }
    }
    public bool AtWanderPoint() {
        Vector3 flatPosition = FlatPosition(transform.position);
        Vector3 flatWander = FlatPosition(wanderPos);
        
        if (!hasWaited && Vector3.Distance(flatPosition, flatWander) < 1f) {
            //Debug.Log("Going to Wait");
            ChangeAnimation(lookAroundState);
            wanderPointSet = false;
            wandering = false;
            waiting = true;
            hasWaited = true;
            StartCoroutine(ResetWaitedFlag());
            return true;
        }
        return false;
    }
    private void SearchWanderPoint() {
        float randomZ = UnityEngine.Random.Range(-wanderRadius, wanderRadius);
        float randomX = UnityEngine.Random.Range(-wanderRadius, wanderRadius);

        wanderPos = new Vector3(transform.position.x + randomX, 0f, transform.position.z + randomZ);
        if (Physics.Raycast(transform.position, (wanderPos - transform.position).normalized, wanderRadius, whatIsWall)) 
        {
            //Debug.Log("Wall detected! Not moving this frame.");
            return; // Exit without changing position
        }
    
        // No wall detected, proceed with movement
        //Debug.Log("Safe path. Moving to wander position.");
        Debug.DrawRay(transform.position, (wanderPos - transform.position).normalized * wanderRadius, Color.green, 1f);
        wanderPointSet = true;
    }

    private void DestroyEnemy() {
    Debug.Log($"{gameObject.name} Defeated!");
    Destroy(gameObject);
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
