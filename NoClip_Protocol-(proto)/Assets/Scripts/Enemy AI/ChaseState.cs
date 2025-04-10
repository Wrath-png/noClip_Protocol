using UnityEngine;

// ChaseState.cs
public class ChaseState : EnemyState
{
    public ChaseState(SmallEnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.agent.isStopped = false;
        enemy.ChangeAnimation("idle");

    }

    public override void Update()
    {
        if (!enemy.CanSeePlayer())
        {
            enemy.TransitionToState(new PatrolState(enemy));
            return;
        }

        if (enemy.InAttackRange())
        {
            enemy.TransitionToState(new AttackState(enemy));
            return;
        }

        enemy.agent.SetDestination(enemy.PlayerPositionFlat());
    }

    public override void Exit()
    {
        // Reset chasing flags or stop animation if necessary
    }
}

