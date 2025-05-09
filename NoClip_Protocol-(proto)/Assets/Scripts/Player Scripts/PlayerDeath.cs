using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDeath : MonoBehaviour
{
    public Image fadeToBlack;
    private float fadeSpeed = 0.5f;
    public GameObject TextObj;
    public TextMeshProUGUI deathText;
    private CanvasGroup deathTextGroup;

    
    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
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
        StartCoroutine(FindDeathText());
    }

    public void Death() {
        StartCoroutine(HandleDeath());
        
    }
    private IEnumerator HandleDeath() {
        StartCoroutine(FadeToBlack());
        ShowDeathText();
        yield return new WaitForSeconds(2f);

        MainMenu menu = FindAnyObjectByType<MainMenu>();
        if (menu != null)
            menu.ShowDeathMenu();
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

    void ShowDeathText() {
        if (deathText == null) return;

        deathText.text = $"You did Not Survive";
        deathTextGroup.alpha = 0f;
        deathText.gameObject.SetActive(true);

        //StopAllCoroutines();
        StartCoroutine(FadeLevelText());
    }

    IEnumerator FadeLevelText() {
        float duration = 1.5f;
        float time = 0f;

        Vector3 originalScale = deathText.transform.localScale * 0.3f;
        Vector3 targetScale = originalScale * 1.2f;

        while (time < duration) {
            float t = time / duration;
            deathTextGroup.alpha = Mathf.Lerp(0f, 1f, t);
            deathText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        deathTextGroup.alpha = 1f;
        deathText.transform.localScale = targetScale;
    }

    IEnumerator FindDeathText() {
        // Wait until the object exists
        while ((TextObj = GameObject.FindGameObjectWithTag("LevelCount")) == null)
            yield return null; // Try again next frame

        //GameObject levelTextObj = GameObject.Find("Level Count");
        
        deathText = TextObj.GetComponent<TextMeshProUGUI>();
        deathTextGroup = deathText.GetComponent<CanvasGroup>();
        if (deathTextGroup == null)
            deathTextGroup = deathText.gameObject.AddComponent<CanvasGroup>();    
    }
}
