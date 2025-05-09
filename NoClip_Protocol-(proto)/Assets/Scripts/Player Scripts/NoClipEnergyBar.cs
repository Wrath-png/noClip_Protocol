using UnityEngine;
using UnityEngine.UI;

public class NoClipEnergyBar : MonoBehaviour
{
    [SerializeField] private Slider energyBar;
    public void UpdateEnergyBar(float currentEnergy, float maxEnergy) {
        if (energyBar == null)
            return;

        float energyPercent = currentEnergy / maxEnergy;
        energyBar.value = energyPercent;
    }
}
