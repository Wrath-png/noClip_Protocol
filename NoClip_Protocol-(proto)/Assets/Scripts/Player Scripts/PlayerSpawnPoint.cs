using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    void Start()
    {
        PlayerActions player = FindAnyObjectByType<PlayerActions>();
        if (player != null)
        {
            player.transform.SetPositionAndRotation(new Vector3(transform.position.x, 10f, transform.position.z), Quaternion.Euler(0f, 180f, 0f));
        }
    }
}
