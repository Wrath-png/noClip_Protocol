using UnityEngine;

// AttackState.cs
public class AttackState : EnemyState
{
    public AttackState(SmallEnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.agent.isStopped = false;
        enemy.agent.SetDestination(enemy.transform.position);
        enemy.FacePlayer();
    }

    public override void Update()
    {
        if (!enemy.CanSeePlayer())
        {
            if (enemy.enableChase && enemy.RememberLocation())
            {
                enemy.TransitionToState(new ChaseState(enemy));
            }
            else
            {
                enemy.TransitionToState(new PatrolState(enemy));
            }
            return;
        }
        
        if (enemy.enableChase && !enemy.InAttackRange())
        {
            enemy.TransitionToState(new ChaseState(enemy));
            return;
        }
        
        enemy.FacePlayer();
        enemy.TryAttack();
    }

    public override void Exit()
    {

    }
}

