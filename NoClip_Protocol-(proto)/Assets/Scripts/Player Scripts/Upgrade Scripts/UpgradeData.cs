using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Upgrades/UpgradeTier")]
public class UpgradeData : ScriptableObject
{
    public string upgradeID;
    public int tier;
    public GameObject prefab;
}
