using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyHealthBar))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int _MaxHealth = 30;
    [SerializeField] private int _Health;
    private EnemyHealthBar healthBar;
    public AudioSource smallEnemySound;
    public AudioSource fxSource;
    public AudioClip explosion;
    public AudioClip damageSound;

    public int CurrentHealth { get => _Health; private set => _Health = value; }

    public int MaxHealth { get => _MaxHealth; private set => _MaxHealth = value; }

    public event IDamageable.TakeDamageEvent OnTakeDamage;
    public event IDamageable.DeathEvent OnDeath;

    void Awake() {
        CurrentHealth = MaxHealth;
        healthBar = GetComponent<EnemyHealthBar>();
        if (healthBar != null) {
            healthBar.UpdateHealthBar(CurrentHealth, MaxHealth);
        }
    }

    public void SetMaxHealth(int newMaxHealth) {
        MaxHealth = newMaxHealth;
        CurrentHealth = newMaxHealth;

        if (healthBar != null)
            healthBar.UpdateHealthBar(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(int Damage)
    {
        fxSource.PlayOneShot(damageSound);

        int damageTaken = Mathf.Clamp(Damage, 0, CurrentHealth);

        CurrentHealth -= damageTaken;
        if (damageTaken != 0) {
            OnTakeDamage?.Invoke(damageTaken);
            if (healthBar != null)
                healthBar.UpdateHealthBar(CurrentHealth, MaxHealth);
        }

        if (CurrentHealth == 0 && damageTaken != 0) {
            OnDeath?.Invoke(transform.position);
            HandleDeath();
        }
    }

    private void HandleDeath() {
        smallEnemySound.clip = explosion;
        smallEnemySound.loop = false;
        smallEnemySound.Play();

        // Disable NavMeshAgent
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // Enable Rigidbody physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;  // Smooth movement
        }

        // Disable AI script
        SmallEnemyAI ai = GetComponent<SmallEnemyAI>();
        if (ai != null) ai.enabled = false;

        // Disable the capsule collider
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null) {
            capsule.enabled = false;
        }

        // Enable the mesh collider and make sure it's not a trigger
        MeshCollider meshCol = GetComponent<MeshCollider>();
        if (meshCol != null) {
        meshCol.enabled = true;
        meshCol.convex = true;  // Required for mesh colliders with Rigidbody
        meshCol.isTrigger = false;
        }

        // Wait 2 seconds, then destroy
        StartCoroutine(DelayedDestroy());
    }

    private IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(2f);
        GameEvents.EnemyDied(); //Notifies when an enemy dies
        Destroy(gameObject);
    }
}