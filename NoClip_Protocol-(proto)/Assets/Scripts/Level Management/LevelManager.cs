using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public GameObject levelTextObj;
    public TextMeshProUGUI levelCountText;
    private CanvasGroup levelTextGroup;
    private Vector3 originalScale;

    [SerializeField, Tooltip("Displays the current level for debugging")]
    private int currentLevelDebug = 1;

    public int CurrentLevel
    {
        get => currentLevelDebug;
        private set => currentLevelDebug = value;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Awake()
    {
        Debug.Log("LevelManager Awake - Is this instance? " + (Instance == null));
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.Log("Destroying duplicate Instance");
            Destroy(gameObject); // Destroy the duplicate
            return;
        }

        //levelCountText = GameObject.Find("RageQuitPlayer").transform.Find("Level Count").GetComponent<TextMeshProUGUI>();
    }

    public void ResetLevel()
    {
        CurrentLevel = 1;
    }
    public void AdvanceLevel()
    {
        CurrentLevel++;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            Destroy(gameObject);
            return;
        }
        if (scene.buildIndex == 1) return; // Skip if it's the main menu
        StopAllCoroutines();
        levelCountText = null;
        levelTextGroup = null;
        StartCoroutine(DelayedInit());
    }
    IEnumerator DelayedInit() {
        // Wait until the object exists
        while ((levelTextObj = GameObject.FindGameObjectWithTag("LevelCount")) == null)
            yield return null; // Try again next frame

        //GameObject levelTextObj = GameObject.Find("Level Count");
        
        levelCountText = levelTextObj.GetComponent<TextMeshProUGUI>();
        levelTextGroup = levelCountText.GetComponent<CanvasGroup>();
        if (levelTextGroup == null)
            levelTextGroup = levelCountText.gameObject.AddComponent<CanvasGroup>();
        
        originalScale = levelCountText.transform.localScale;
            
        ShowLevelText();
            
        
    }

    void ShowLevelText() {
        if (levelCountText == null) return;

        levelCountText.text = $"Level {CurrentLevel}";
        levelTextGroup.alpha = 1f;
        levelCountText.transform.localScale = originalScale;
        levelCountText.gameObject.SetActive(true);

        //StopAllCoroutines();
        StartCoroutine(FadeLevelText());
    }

    IEnumerator FadeLevelText() {
        float duration = 1.5f;
        float time = 0f;

        while (time < duration)
        {
            if (levelTextGroup == null || levelCountText == null)
                yield break;    //Should orevent exception when destroyed on reset.

            float t = time / duration;
            levelTextGroup.alpha = Mathf.Lerp(1f, 0f, t);
            levelCountText.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.5f, t);
            time += Time.deltaTime;
            yield return null;
        }

        levelTextGroup.alpha = 0f;
        levelCountText.gameObject.SetActive(false);
    }
}
