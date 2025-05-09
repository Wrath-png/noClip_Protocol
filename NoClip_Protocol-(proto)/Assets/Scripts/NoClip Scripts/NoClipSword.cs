using UnityEngine;

public class NoClipSword : MonoBehaviour
{
    public Animator swordAnimator;
    public AudioSource soundSource;
    public AudioClip swordSound;
    public int damage = 50;

    void Start()
    {
        soundSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            soundSource.PlayOneShot(swordSound);
            swordAnimator.SetTrigger("Swing");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the other object has the IDamageable component
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Deal damage when it hits a damageable entity
            damageable.TakeDamage(damage);
        }
    }

    public void UpgradeDamage(UpgradeData upgrade) {
        int bonusDamage = 10 * upgrade.tier;
        damage += bonusDamage;
        Debug.Log($"Applied NoClip Damage Upgrade. New NoClip Sword Damage: {damage}");
    }
}
