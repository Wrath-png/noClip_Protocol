using UnityEngine;

// AttackState.cs
public class AttackState : EnemyState
{
    public AttackState(SmallEnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.agent.SetDestination(enemy.transform.position);
        enemy.FacePlayer();
    }

    public override void Update()
    {
        if (!enemy.InAttackRange())
        {
            enemy.TransitionToState(new ChaseState(enemy));
            return;
        }

        enemy.FacePlayer();
        enemy.TryAttack();
    }

    public override void Exit()
    {
        // Reset attack flags if needed
    }
}

