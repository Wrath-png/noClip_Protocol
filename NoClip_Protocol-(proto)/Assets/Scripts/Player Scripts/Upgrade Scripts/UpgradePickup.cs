using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UpgradePickup : MonoBehaviour
{
    public UpgradeData upgradeData;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerUpgradeTracker tracker = other.GetComponent<PlayerUpgradeTracker>();
            PlayerActions player = other.GetComponent<PlayerActions>();

            if (tracker != null && player != null && upgradeData != null) {
                tracker.CollectUpgrade(upgradeData);
                player.ApplyUpgrade(upgradeData);
                Destroy(gameObject); // Remove the pickup from the world
            }
        }
    }
}
