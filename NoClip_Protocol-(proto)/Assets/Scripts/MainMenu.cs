using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public BGM bgm;
    void Start()
    {
    
    }

    //Loads up the game
    public void Play() {
        bgm.SetMusic("Kaixo - Mystic");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    //Quits the game
    public void Quit() {
        Application.Quit();
        Debug.Log("Player Has Closed The Game");
    }
}
