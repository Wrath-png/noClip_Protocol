using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

//Will contain the AI for the easy enemies. They will travel from waypoint to waypoint but stop and shoot at the player if they see them.
public class SmallEnemyAI : MonoBehaviour
{
    public float speed = 15;
    [SerializeField] float curHealth, maxHealth = 25;
    [SerializeField] EnemyHealthBar healthBar;
    public NavMeshAgent agent;
    public Transform player;
    public Transform head;
    public LayerMask whatIsWall, whatIsPlayer;
    public Transform[] waypoints;
    
    [Header("Animation")]
    public AnimationStateChanger animationStateChanger;
    public string idleState = "idle";
    public string lookAroundState = "LookAround";
    [Space]
    [Header("AI Properties")]
    //Patroling
    private int currentWaypointIndex;
    public float waitTime = 2f;     //Seconds
    private float waitCounter = 0f;
    private bool waiting = false;

    //Chasing
    private bool chasing = false;
    [SerializeField] private float chaseMemoryTime = 2f; // How long to remember the player
    private float lastSeenTime = Mathf.NegativeInfinity;

    //Attacking
    public float timeBetweenAttacks;
    bool attacking;

    //Wandering
    public float walkRange;
    private Vector3 wanderPos;
    private bool wanderPointSet = false;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInLineOfSight, playerInAttackRange = false;

    private void Awake() {
        healthBar = GetComponentInChildren<EnemyHealthBar>(true);   
        //(true) will search all nested children, even inactive objects.
    }
    void Start()
    {
        animationStateChanger = GetComponent<AnimationStateChanger>();
        agent = GetComponent<NavMeshAgent>();
        head = transform.Find("ProtoEnemy");
        player = GameObject.Find("RagequitPlayer").transform;

        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);

    }

    void Update()
    {
        //Checks for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (playerInSightRange && PlayerLineOfSight()) {
            if (playerInAttackRange) {
                Attack();
            }
            else {
                Chase();
            }
        }
        else if (chasing && (Time.time - lastSeenTime) <= chaseMemoryTime) {
            Chase(); // Continue chasing for a short period
        }
        else {
            chasing = false;  // Reset if player is out of sight and memory time expires
            Patrol();
        }
    }

    bool PlayerLineOfSight() {
        Vector3 direction = (player.position - head.position).normalized;

        float angle = Vector3.Angle(head.forward, direction);  //Finds angle between enemy's forward and direction of player
        if (angle < 90) {

            float distance = Vector3.Distance(head.position, player.position);
            if (Physics.Raycast(head.position, direction, distance, whatIsWall)) {
                return playerInLineOfSight = false;  // Return false if it hits anything that isn't the player
            }
            else if (Physics.Raycast(head.position, direction, distance, whatIsPlayer)) {
                ;
                // If the ray reaches the player, return true
                lastSeenTime = Time.time;
                if(!chasing) {
                agent.isStopped = false;
                ChangeAnimation(idleState);     //Ends look around animation 
                //transform.LookAt(player);       //Makes enemy look at player to ensure attention is not lost
                chasing = true;                 //Sets chasing to true so that it doesn't restart the patrol
                }
                return playerInLineOfSight = true;
            }
        
        }
        return playerInLineOfSight = false;   //Player not in field of vision.
    }


    void ChangeAnimation (string newAnimationState) {
        animationStateChanger.ChangeAnimationState(newAnimationState);
    }

    void Patrol() {
        //Debug.Log("Began Patroling");
        if(waiting) {
            //Debug.Log("In waiting");
            waitCounter += Time.deltaTime;
            if(waitCounter < waitTime) {
                return;
            }
            else {
                //Debug.Log("Leaving waiting");
                waiting = false;
                ChangeAnimation(idleState);
            }
        }

        Transform wp = waypoints[currentWaypointIndex];
        //Debug.Log("Current Waypoint: " + currentWaypointIndex);
        //Ensures trasnform positions ignore the y axis
        Vector3 flatPosition = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatWaypoint = new Vector3(wp.position.x, 0f, wp.position.z);

        if (Vector3.Distance(flatPosition, flatWaypoint) < 0.01f) {
            //transform.position = new Vector3(wp.position.x, transform.position.y, wp.position.z);
            waitCounter = 0f;
            ChangeAnimation(lookAroundState);
            waiting = true;
            agent.isStopped = true;
            //Debug.Log("Agent Is Stopped: " + agent.isStopped);
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        else {
            Vector3 targetPosition = new Vector3(wp.position.x, 0f, wp.position.z);
            //Debug.Log("Target Position: " + targetPosition);
            ChangeAnimation(idleState);
            agent.isStopped = false;
            if (!agent.SetDestination(targetPosition)) {
                Debug.LogError("Failed to set destination!");
            }
            
            // Vector3 direction = (wp.position - transform.position).normalized;
            // controller.Move(direction * speed * Time.deltaTime);
        }
    }

    private void Chase() {
        chasing = true;
        waiting = false;
        waitCounter = waitTime;
        ChangeAnimation(idleState);
        agent.isStopped = false;
        if (!agent.SetDestination(new Vector3(player.position.x, 0f, player.position.z))) {
            Debug.LogError("Failed to set destination!");
        }
    }
    private void Attack() {
        //enemy shouldn't move when attacking
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if(!attacking) {
            ///Atacking code
            attacking = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);    //Will fire lasers with intervals = timeBetweenAttacks
        }
    }
    private void ResetAttack() {
        attacking = false;
    }

    private void Wander() {
        if(!wanderPointSet) SearchWanderPoint();

        //Start moving
        if (wanderPointSet) {
            agent.SetDestination(wanderPos);
        }
        Vector3 distanceToMove = transform.position - wanderPos;

        //WanderPosition reached
        if(distanceToMove.magnitude < 1f) {
            wanderPointSet = false;
        }
    }
    private void SearchWanderPoint() {
        float randomZ = Random.Range(-walkRange, walkRange);
        float randomX = Random.Range(-walkRange, walkRange);

        wanderPos = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        if (Physics.Raycast(transform.position, (wanderPos - transform.position).normalized, walkRange, whatIsWall)) 
        {
            //Debug.Log("Wall detected! Not moving this frame.");
            return; // Exit without changing position
        }
    
        // No wall detected, proceed with movement
        //Debug.Log("Safe path. Moving to wander position.");
        Debug.DrawRay(transform.position, (wanderPos - transform.position).normalized * walkRange, Color.green, 1f);
        wanderPointSet = true;
        transform.LookAt(wanderPos);
    }

    // Contact Damage
    // private void OnTriggerEnter(Collider other) {
    //     if (other.CompareTag("Player")) {
    //         Debug.Log("Hit the player!");
    //         PlayerActions actions = other.GetComponent<PlayerActions>();
    //         if (actions != null) {
    //             actions.TakeDamage(contactDamage);
    //         }
    //     }
    // }

    public void TakeDamage(int damage) {
        curHealth -= damage;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        Debug.Log($"Enemy {gameObject.name} took {damage} damage. Current Health: {curHealth}/{maxHealth}");

        if (curHealth <= 0) {

            //Add code to stop all movement and animations

            Invoke("DestroyEnemy", 0.5f);
        }
    }
    private void DestroyEnemy() {

        //Change code to instead apply gravity to enemy so it falls to the ground.

        Debug.Log($"{gameObject.name} Defeated!");
        Destroy(gameObject);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
