using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

//Will contain the AI for the easy enemies. They will travel from waypoint to waypoint but stop and shoot at the player if they see them.
public class SmallEnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 15;
    [SerializeField] public float maxHealth = 25;
    private float curHealth;

    [Header("AI Settings")]
    public float sightRange, attackRange;
    public float atkDamage;
    public float waitTime = 2f;
    public float timeBetweenAttacks = 1f;
    public float chaseMemoryTime = 2f;
    public float walkRange = 10f;

    [Header("References")]
    public Transform[] waypoints;
    public Transform head;
    public LayerMask whatIsWall, whatIsPlayer;
    public AnimationStateChanger animationStateChanger;
    [SerializeField] EnemyHealthBar healthBar;

    public NavMeshAgent agent { get; private set; }
    public Transform player { get; private set; }

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
        healthBar = GetComponentInChildren<EnemyHealthBar>(true);
        head = transform.Find("ProtoEnemy");
        player = GameObject.Find("RagequitPlayer").transform;
    }
    void Start()
    {
        animationStateChanger = GetComponent<AnimationStateChanger>();
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        TransitionToState(new PatrolState(this));
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
                    lastSeenTime = Time.time;
                    ChangeAnimation(idleState);     //Ends look around animation 
                    return true;
                }
                // If the ray reaches the player, return true
                //lastSeenTime = Time.time;
                //if(!chasing) {
                //agent.isStopped = false;
                //ChangeAnimation(idleState);     //Ends look around animation 
                //transform.LookAt(player);       //Makes enemy look at player to ensure attention is not lost
                //chasing = true;                 //Sets chasing to true so that it doesn't restart the patrol
            }
        }
        return false;   //Player not in field of vision.
    }
    public float TimeSinceLastSeenPlayer() {
        return Time.time - lastSeenTime;
    }
    public bool InAttackRange() {
        return Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
    }
    public Vector3 PlayerPositionFlat() {
        return new Vector3(player.position.x, 0f, player.position.z);
    }
    public void FacePlayer() {
        transform.LookAt(PlayerPositionFlat());
    }
    public void TryAttack() {
        if (!attacking) {
            attacking = true;
            // Hook up to EnemyGun logic here
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
        Vector3 flatPosition = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatWaypoint = new Vector3(wp.position.x, 0f, wp.position.z);
        
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
    public void WaitAtWaypoint(){
        if(waiting) {
            waitCounter += Time.deltaTime;
            if(waitCounter < waitTime) {
                return;
            }
            else {
                waiting = false;
                ChangeAnimation(idleState);
                waitCounter = 0f;

                SetNextWaypoint();
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
