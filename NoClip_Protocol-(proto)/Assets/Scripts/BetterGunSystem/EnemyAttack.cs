using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyAttack : MonoBehaviour 
{
    [SerializeField] private GunSelector gunSelector;

    public void Attack() 
    {
        if (gunSelector.ActiveGun != null) {
            SmallEnemyAI enemyAI = GetComponentInParent<SmallEnemyAI>();  // Assuming SmallEnemyAI is on the same GameObject or parent
            Debug.Log($"Attacking with gun: {gunSelector.ActiveGun.name}, Enemy: {enemyAI.gameObject.name}");

            if (enemyAI != null) {
        
                gunSelector.ActiveGun.Shoot(enemyAI.GetDamageMultiplier());
            } else {
                gunSelector.ActiveGun.Shoot(1);  // Default behavior if no SmallEnemyAI is found
            }
        }
    }
}