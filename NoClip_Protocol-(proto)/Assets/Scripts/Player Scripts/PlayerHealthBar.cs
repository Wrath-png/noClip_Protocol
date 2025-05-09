using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private float healthPercent;
    private Image fillImage;

    public void UpdateHealthBar(float curHealth, float maxHealth) {
        if (fillImage == null) {
            //UpdateHealthBar keeps getting called before fillImage is referenced
            //Moving the reference here ensures that mistake won't happen
            fillImage = healthBar.fillRect.GetComponent<Image>();
        }

        healthPercent = curHealth / maxHealth;
        healthBar.value = healthPercent;
        
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fillImage = healthBar.fillRect.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
