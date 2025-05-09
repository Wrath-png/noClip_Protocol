using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.SetPositionAndRotation(new Vector3(transform.position.x, 10f, transform.position.z), Quaternion.Euler(0f, 180f, 0f));
        }
    }
}
