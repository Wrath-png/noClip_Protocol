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
            if (!enemy.RememberLocation()) 
            {
                enemy.TransitionToState(new PatrolState(enemy));
                return;
            }
        }

        if (enemy.enableAttack && enemy.InAttackRange())
        {
            enemy.TransitionToState(new AttackState(enemy));
            return;
        }

        enemy.agent.SetDestination(enemy.PlayerPositionFlat());
    }

    public override void Exit()
    {

    }
}

