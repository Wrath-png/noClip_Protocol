
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class HitboxMover : MonoBehaviour
{
    private GameObject owner;
    private DamageConfigScriptableObject damageConfig;
    private Vector3 direction;
    private float speed;
    private float damageMult;

    private float lifetime = 5f; // Failsafe lifetime
    private float timer = 0f;

    private bool hasCollided = false;

    public void Initialize(Vector3 start, Vector3 direction, DamageConfigScriptableObject config, float speed, float multiplier)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.damageConfig = config;
        this.damageMult = multiplier;

        transform.position = start;
    }

    public void SetOwner(GameObject owner) {
        this.owner = owner;
    }

    void Update()
    {
        if (hasCollided) return;

        transform.position += direction * speed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (hasCollided) return;

        // Avoid self-hit or friendly fire
        if (other.gameObject == owner || other.transform.IsChildOf(owner.transform))
        {
            return;
        }

        hasCollided = true;
        int finalDamage = Mathf.RoundToInt(damageMult * damageConfig.GetDamage(1f));

        if (other.CompareTag("Player"))
        {
            PlayerActions player = other.GetComponent<PlayerActions>();
            if (player != null)
            {
                player.TakeDamage(finalDamage);
            }
        }
        else if (other.TryGetComponent<IDamageable>(out IDamageable target))
        {
            target.TakeDamage(finalDamage);
        }
        //TODO: Add VFX here (Sparks, Scorch Marks, Sounds)
        Destroy(gameObject);
    }
}
