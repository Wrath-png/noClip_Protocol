using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpgradeData))]
public class UpgradeDataEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpgradeData data = (UpgradeData)target;

        if (data.prefab != null)
        {
            if (GUILayout.Button("Auto-Configure Prefab"))
            {
                AddRequiredComponents(data.prefab);
            }
        }
    }

    private void AddRequiredComponents(GameObject prefab)
    {
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabInstance = (GameObject)PrefabUtility.LoadPrefabContents(prefabPath);

        if (prefabInstance.GetComponent<Collider>() == null)
        {
            prefabInstance.AddComponent<BoxCollider>().isTrigger = true;
            Debug.Log("Added BoxCollider.");
        }

        if (prefabInstance.GetComponent<UpgradePickup>() == null)
        {
            prefabInstance.AddComponent<UpgradePickup>();
            Debug.Log("Added UpgradePickup.");
        }

        UpgradePickup pickup = prefabInstance.GetComponent<UpgradePickup>();
        UpgradeData data = (UpgradeData)target;
        pickup.upgradeData = data;

        PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabInstance);

        Debug.Log("Prefab updated successfully.");
    }
}
