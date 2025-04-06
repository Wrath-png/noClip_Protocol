using UnityEngine;

public class PracticeDummy : MonoBehaviour
{
    [SerializeField] float curHealth, maxHealth = 100;
    [SerializeField] EnemyHealthBar healthBar;

    // Function to take damage
   public void TakeDamage(int damage) {
        curHealth -= damage;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        Debug.Log($"{gameObject.name} took {damage} damage. Current Health: {curHealth}/{maxHealth}");

        if (curHealth <= 0) {
            Invoke("DestroyDummy", 0.5f);
        }
    }

    // Function to destroy the dummy when health reaches zero
    private void DestroyDummy()
    {
        // Add any destruction effects here if necessary (e.g., animations, sound effects)
        Destroy(gameObject);
    }

    void Start()
    {
        healthBar = GetComponentInChildren<EnemyHealthBar>(true);   
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
    }
}
