using UnityEngine;

public class WinCondition : MonoBehaviour
{
    public int requiredKills = 10; // Set in inspector
    private int currentKills = 0;

    public GameObject exitDoor; // Reference to your exit door / trigger

    private void OnEnable()
    {
        GameEvents.OnEnemyDied += OnEnemyDied;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDied -= OnEnemyDied;
    }

    private void OnEnemyDied()
    {
        currentKills++;
        Debug.Log($"Enemy killed. Total: {currentKills}");

        if (currentKills >= requiredKills)
        {
            OpenExit();
        }
    }

    private void OpenExit()
    {
        Debug.Log("Win condition met. Opening exit.");
        exitDoor.SetActive(false); // Or trigger animation, etc.
    }

    void Start()
    {
        exitDoor = GameObject.FindGameObjectWithTag("ExitDoor");
        exitDoor.SetActive(true);
    }
}
