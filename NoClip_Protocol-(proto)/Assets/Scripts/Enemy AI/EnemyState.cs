using UnityEngine;

public abstract class EnemyState
{
    protected SmallEnemyAI enemy;

    public EnemyState(SmallEnemyAI enemy) 
    {
        this.enemy = enemy;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
