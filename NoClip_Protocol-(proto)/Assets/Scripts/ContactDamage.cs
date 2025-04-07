using System.Collections;
using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    public int damage = 1;
    public float interval = 1f;

    private Coroutine damageRoutine;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            var player = other.GetComponent<PlayerActions>();
            if (player != null)
                damageRoutine = StartCoroutine(DamageOverTime(player));
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && damageRoutine != null) {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }
    }

    private IEnumerator DamageOverTime(PlayerActions player) {
        while (true) {
            player.TakeDamage(damage);
            yield return new WaitForSeconds(interval);
        }
    }
}
