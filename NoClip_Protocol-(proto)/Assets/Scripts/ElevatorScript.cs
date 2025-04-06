using System.Collections;
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
    public float liftSpeed = 2f;
    public float fadeSpeed = 0.5f;
    public float liftHeight = 50f;
    [SerializeField] private bool isActivated = false;

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
        Vector3 newPosition = new Vector3(playerTargetPosition.position.x, player.transform.position.y, playerTargetPosition.position.z);
        player.transform.position = newPosition;

        player.transform.rotation = Quaternion.Euler(0, 90, 0);

        //Parent the player to the elevator while maintaining their relative position
        player.transform.SetParent(transform, true);

        // Lift the entire winZone (which includes the elevator plane)
        Vector3 targetPosition = transform.position + Vector3.up * liftHeight;
        while (transform.position.y < targetPosition.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, liftSpeed * Time.deltaTime);
            yield return null;
        }

        // Fade to black
        yield return StartCoroutine(FadeToBlack());

        // Restart the game
        SceneManager.LoadScene(1);
    }

    private IEnumerator FadeToBlack()
    {
        Color startColor = new Color(0, 0, 0, 0);
        while (startColor.a < 1) {
            startColor.a += fadeSpeed * Time.deltaTime;
            fadeToBlack.color = startColor;
            yield return null;
        }
    }
}
