using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

//Will spawn selection of upgrades at spawn points around the map
//The upgrade is random at each station

public class UpgradesScript : MonoBehaviour
{
    public UpgradeData[] allUpgrades; // Assign all possible upgrades in inspector
    public Transform[] spawnPoints;
    public PlayerUpgradeTracker playerUpgradeTracker;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 2)
        {
            return;  //Skip if not in levelOne
        }

        if (playerUpgradeTracker == null)
        {
            playerUpgradeTracker = FindAnyObjectByType<PlayerUpgradeTracker>();
        }

        spawnPoints = null;
        spawnPoints = GameObject.FindGameObjectsWithTag("UpSpawn").Select(obj => obj.transform).ToArray();
        SpawnUpgrades();

    }

    void SpawnUpgrades()
    {
        // Group upgrades by upgradeID
        var grouped = allUpgrades.GroupBy(u => u.upgradeID);
        int index = 0;
        HashSet<string> spawnedUpgradeIDs = new HashSet<string>();

        foreach (var upgradeGroup in grouped)
        {
            string id = upgradeGroup.Key;
            int currentTier = playerUpgradeTracker.GetCurrentTier(id);

            if (currentTier == 0) {
                // Get the tier 1 upgrade
                UpgradeData tierOneUpgrade = upgradeGroup.FirstOrDefault(u => u.tier == 1);
                if (tierOneUpgrade != null && index < spawnPoints.Length) {
                    Instantiate(tierOneUpgrade.prefab, spawnPoints[index].position, Quaternion.identity);
                    index++;
                    Debug.Log($"Spawning Tier 1 upgrade: {tierOneUpgrade.name}, ID: {tierOneUpgrade.upgradeID}");
                }
            }
            else if (currentTier > 0) {
                // Attempt to get the next tier (or Tier 1 if none collected)
                UpgradeData nextUpgrade = upgradeGroup.FirstOrDefault(u => u.tier == currentTier + 1);
                if (nextUpgrade != null && index < spawnPoints.Length && !spawnedUpgradeIDs.Contains(nextUpgrade.upgradeID)) {
                    Instantiate(nextUpgrade.prefab, spawnPoints[index].position, Quaternion.identity);
                    spawnedUpgradeIDs.Add(nextUpgrade.upgradeID);
                    index++;
                }
                else {
                    Debug.Log("There is no more upgrades for this stat");
                }
            }
        }
    }

}
