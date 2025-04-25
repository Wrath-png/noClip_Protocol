using UnityEngine;

public class WanderState : EnemyState
{
    private float wanderTimer = 0f;  // Timer to track wandering duration
    public WanderState(SmallEnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.agent.isStopped = false;
        enemy.ChangeAnimation("idle");
        wanderTimer = 0f;
        //enemy.Wander();
    }

    public override void Update()
    {
        if (enemy.enableChase && enemy.CanSeePlayer()) {
            enemy.TransitionToState(new ChaseState(enemy));
            return;
        }
        if (enemy.waiting) {
            enemy.WaitAtPoint();
            return;
        }
        if (enemy.AtWanderPoint() && !enemy.hasWaited) {
            return;
        }
        if (wanderTimer >= enemy.wanderTime) {
            if (enemy.enablePatrol) {
                enemy.TransitionToState(new PatrolState(enemy));
            }
            else{
                // Keep wandering indefinitely if patrol is disabled
                wanderTimer = 0f;  // Reset timer to keep wandering
                //enemy.Wander();  // Restart wandering
            }
        }
        else {
            // Update the timer while wandering
            wanderTimer += Time.deltaTime;
            enemy.Wander();

        }
    }
    public override void Exit()
    {

    }
}
