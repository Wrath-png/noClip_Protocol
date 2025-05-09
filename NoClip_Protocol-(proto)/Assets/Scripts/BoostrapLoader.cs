using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadGameStartScene());
    }

    IEnumerator LoadGameStartScene()
    {
        yield return null;
        SceneManager.LoadScene(1); 
    }
}
