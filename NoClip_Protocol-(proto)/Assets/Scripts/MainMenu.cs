using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    private static MainMenu instance;
    private GameObject prevMenu;
    public GameObject mainMenuUI, pauseMenuUI, pauseBlurVolume, optionsMenuUI, optionsBackground; // Assign the menu panel here
    public static bool IsPaused { get; private set; } = false;
    private bool isPaused = false;
    public BGM bgm;

    void Awake() 
    {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keeps this across scenes
        }
        else {
            Destroy(gameObject); // Prevent duplicates
        }
    }
    void Start() 
    {
    pauseMenuUI.SetActive(false);
    optionsMenuUI.SetActive(false);
    mainMenuUI.SetActive(true);
    Time.timeScale = 1f;
    }

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }
    public void TogglePause() {
        isPaused = !isPaused;
        IsPaused = isPaused; // <-- update static flag
        pauseMenuUI.SetActive(isPaused);

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        pauseBlurVolume.SetActive(isPaused);
    }
    public void ResumeGame()
    {
        isPaused = false;
        IsPaused = false;

        pauseMenuUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    //Loads up the game
    public void Play() {
        bgm.SetMusic("Kaixo - Mystic");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        mainMenuUI.SetActive(false);
    }

    //Quits the game
    public void Quit() {
        Application.Quit();
        Debug.Log("Player Has Closed The Game");
    }

    public void OpenOptionsFromMain() {
        prevMenu = mainMenuUI;
        mainMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
        optionsBackground.SetActive(true);
    }
    public void OpenOptionsFromPause() {
        prevMenu = pauseMenuUI;
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
        optionsBackground.SetActive(false);
    }
    public void BackFromOptions() {
        optionsMenuUI.SetActive(false);
        if (prevMenu != null)
            prevMenu.SetActive(true);
    }
}
