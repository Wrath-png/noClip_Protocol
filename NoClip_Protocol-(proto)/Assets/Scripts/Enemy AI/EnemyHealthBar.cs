using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private float healthPercent;
    private Image fillImage;

    public void UpdateHealthBar(float curHealth, float maxHealth) {
        if (healthBar == null) {
            healthBar = GetComponentInChildren<Slider>();
        }
        if (fillImage == null) {
            fillImage = healthBar.fillRect.GetComponent<Image>();
        }
        
        healthBar.value = curHealth / maxHealth;
        healthPercent = curHealth / maxHealth;

        if (healthPercent > 0.8)
            fillImage.color = Color.green;
        else if (healthPercent > 0.6)
            fillImage.color = Color.Lerp(Color.yellow, Color.green, (healthPercent - 0.6f) * 2);
        else if (healthPercent > 0.4)
            fillImage.color = Color.yellow;
        else if (healthPercent > 0.2)
            fillImage.color = Color.Lerp(Color.red, Color.yellow, (healthPercent - 0.2f) * 2);
        else
            fillImage.color = Color.red;
        
    }

    void Awake() {
        if (healthBar == null) {
            healthBar = GetComponentInChildren<Slider>();
        }

        if (healthBar != null && fillImage == null) {
            fillImage = healthBar.fillRect.GetComponent<Image>();
        }
    }
}
