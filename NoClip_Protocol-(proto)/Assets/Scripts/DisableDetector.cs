using UnityEngine;

public class DisableDetector : MonoBehaviour
{
    private bool wasActive = true;

    void Update()
    {
        if (!gameObject.activeSelf && wasActive)
        {
            wasActive = false;
            Debug.LogError("GAMEOBJECT DISABLED: " + gameObject.name + " at frame " + Time.frameCount + "\nStackTrace:\n" + System.Environment.StackTrace);
        }
    }

    void OnEnable()
    {
        wasActive = true;
    }
}
