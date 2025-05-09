using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeEntry {
    public UpgradeData upgrade;
    public int tier;
}

public class PlayerUpgradeTracker : MonoBehaviour
{
    [SerializeField] private List<UpgradeEntry> upgrades = new(); // serialized and visible in Inspector

    private Dictionary<string, int> collectedUpgrades = new();

    public AudioSource collectSource;
    public AudioClip collectSound;

    private void Awake()
    {
        // Rebuild dictionary from list
        collectedUpgrades.Clear();
        foreach (var entry in upgrades)
        {
            if (entry.upgrade != null)
            {
                collectedUpgrades[entry.upgrade.upgradeID] = entry.tier;
            }
        }
    }

    public void CollectUpgrade(UpgradeData upgrade)
    {
        collectSource.PlayOneShot(collectSound);
        string ID = upgrade.upgradeID;
        int tier = upgrade.tier;

        if (!collectedUpgrades.ContainsKey(ID) || collectedUpgrades[ID] < tier)
        {
            collectedUpgrades[ID] = tier;

            // Update serialized list
            var existing = upgrades.Find(e => e.upgrade.upgradeID == ID);
            if (existing != null)
            {
                existing.tier = tier;
            }
            else
            {
                upgrades.Add(new UpgradeEntry { upgrade = upgrade, tier = tier });
            }
        }
    }

    public int GetCurrentTier(string upgradeID)
    {
        return collectedUpgrades.TryGetValue(upgradeID, out int tier) ? tier : 0;
    }
}
