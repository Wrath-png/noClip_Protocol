using UnityEngine;

public class PatrolState : EnemyState
{
    public PatrolState(SmallEnemyAI enemy) : base(enemy) { }
    public override void Enter()
    {
        enemy.agent.isStopped = false;
        enemy.SetNextWaypoint();
        enemy.ChangeAnimation("idle");

    }

    public override void Update()
    {
        if (enemy.CanSeePlayer()) {
            enemy.TransitionToState(new ChaseState(enemy));
            return;
        }
        if (enemy.waiting) {
            enemy.WaitAtWaypoint();
            return;
        }
        if (enemy.AtWaypoint() && !enemy.hasWaited) {
            return;
        }
        else {
            enemy.agent.SetDestination(enemy.CurrentWaypointPosition());
        }
    }

    public override void Exit()
    {
        
    }
}
