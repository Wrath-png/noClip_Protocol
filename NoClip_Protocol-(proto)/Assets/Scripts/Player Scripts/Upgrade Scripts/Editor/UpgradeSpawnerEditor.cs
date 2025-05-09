using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(UpgradesScript))]
public class UpgradeSpawnerEditor : Editor
{
    void OnEnable()
    {
        AssignUpgradeData();
    }

    private void AssignUpgradeData() {
        // Get the target object (the UpgradesScript component)
        UpgradesScript upgradesScript = (UpgradesScript)target;

        // Find all UpgradeData assets in the project
        string[] guids = AssetDatabase.FindAssets("t:UpgradeData");

        // Find the actual UpgradeData objects from the guids
        UpgradeData[] allUpgrades = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<UpgradeData>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToArray();

        // Assign the found UpgradeData objects to the allUpgrades array
        upgradesScript.allUpgrades = allUpgrades;

        // Mark the object as dirty to save changes
        EditorUtility.SetDirty(upgradesScript);
    }
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        // Optionally add a button to re-run the assignment manually
        if (GUILayout.Button("Assign All Upgrade Data"))
        {
            AssignUpgradeData();
        }
    }
}
