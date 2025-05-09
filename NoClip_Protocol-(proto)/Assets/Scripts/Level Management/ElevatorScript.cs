using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


//Eventualy update with different next level animations
/*
First: Get on Elevator, walk to center, turn around, slight camshake as elevator rises and screen fades to black
Default: Implement LevelStartSequence animation but in reverse when leaving the level.
Second: Camera moves to be right in front of elevator for 3rd person perspective of getting into elevator, Big screen shake before explosion and sending elevator up at high speed
*/
public class ElevatorScript : MonoBehaviour
{
    public Transform playerTargetPosition;      //Center of Elevator
    public Image fadeToBlack;
    private float liftSpeed = 10f;
    private float fadeSpeed = 0.5f;
    private float liftHeight = 20f;
    [SerializeField] private bool isActivated = false;

    void Start()
    {
        if (fadeToBlack == null) {
            GameObject fadeObj = GameObject.Find("Fade To Black");
            if (fadeObj != null)
            {
                fadeToBlack = fadeObj.GetComponent<Image>();
            }
        else
            {
                Debug.LogWarning("Fade To Black object not found in scene.");
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated && other.CompareTag("Player"))
        {
            isActivated = true;
            StartCoroutine(ActivateElevator(other.gameObject));
        }
    }

    private IEnumerator ActivateElevator(GameObject player) {
        PlayerActions playerActions = player.GetComponent<PlayerActions>();
        if (playerActions != null) {
            playerActions.enabled = false;
        }

        player.transform.rotation = Quaternion.Euler(0, 270, 0);

        //Move player to the center of the elevator
        Vector3 newPosition = new Vector3(playerTargetPosition.position.x, player.transform.position.y+10, playerTargetPosition.position.z);
        player.transform.position = newPosition;

        player.transform.rotation = Quaternion.Euler(0, 90, 0);

        // Fade to black
        StartCoroutine(FadeToBlack());

        // Lift the entire winZone (which includes the elevator plane)
        Vector3 targetPosition = transform.position + Vector3.up * liftHeight;
        float initialPlayerYPosition = player.transform.position.y;
        while (transform.position.y < targetPosition.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, liftSpeed * Time.deltaTime);

            float newPlayerYPosition = initialPlayerYPosition + (transform.position.y - initialPlayerYPosition);
            player.transform.position = new Vector3(player.transform.position.x, newPlayerYPosition, player.transform.position.z);
            yield return null;
        }

       // Once lifted, wait a moment before changing scene
        yield return new WaitForSeconds(1f);

        //Advance level and reload scene
        playerActions.enabled = true;
        fadeToBlack.color = new Color(0, 0, 0, 0);
        LevelManager.Instance.AdvanceLevel();
        SceneManager.LoadScene(2);
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeToBlack != null) {
            Color startColor = new(0, 0, 0, 0);
            while (startColor.a < 1) {
                startColor.a += fadeSpeed * Time.deltaTime;
                fadeToBlack.color = startColor;
                yield return null;
            }
        }
    }
}
