using System.Collections;
using UnityEngine;

public class RestArea : MonoBehaviour
{
    PlayerHealthBar healthBar;
    private void OnTriggerEnter(Collider other)
    {
        PlayerActions player = other.GetComponent<PlayerActions>();
        healthBar = FindAnyObjectByType<PlayerHealthBar>();
        if (player != null)
        {
            StartCoroutine(HealOverTime(player));
        }
    }

    private IEnumerator HealOverTime(PlayerActions player) {
        float totalHealed = 0f;
        float duration = 10f;
        float timeElapsed = 0f;
        float maxHealing = 50f;

        while (totalHealed < maxHealing && player != null)
        {
            // Calculate how much to heal this frame
            float currentHealRate = Mathf.Lerp(1f, 10f, timeElapsed / duration); // Linearly increase from 1 to 10
            float healThisFrame = currentHealRate * Time.deltaTime;

            player.curHealth += healThisFrame;
            healthBar.UpdateHealthBar(player.curHealth, player.maxHealth);
            totalHealed += healThisFrame;
            timeElapsed += Time.deltaTime;

            yield return null;
        }
    }
}
