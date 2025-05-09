using System;

public class GameEvents
{
    public static event Action OnEnemyDied;

    public static void EnemyDied()
    {
        OnEnemyDied?.Invoke();
    }
}
