using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//When the NoCLip affect activates, it will slow down time to 75%
//It will make walls see-thorugh while effect is active
//It will change the player's weapon to a sword whil effect is activated
//It will send out a burst effect that alerts enemies near player and causes them to chase
//It will allow for medium strength and higher enemies to see the player though walls while effect is active
//BONUS: Sets gravity to zero and only allows player to move via sword swing dashes off of a surface.
public class NoClipEffect : MonoBehaviour
{
    public GunSystem gunSystem;
    public GameObject leftMuzzle;
    public GameObject leftShotgun;
    public GameObject rightMuzzle;
    public GameObject rightShotgun;
    public GameObject noClipSword;

    [Header("Transparent Effect")]
    public Material transparentMaterial;
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
    public float fadeDuration = 0.5f;

    [Header("Intangibility Effect")]
    public CharacterController controller;
    private Vector3 noClipStartPosition;
    public bool isInNullZone = false;
    private GameObject[] walls;

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshWalls();
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshWalls();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void NoClipToggle(bool state) {
        gunSystem.enabled = !state;
        leftMuzzle.SetActive(!state);
        leftShotgun.SetActive(!state);
        rightMuzzle.SetActive(!state);
        rightShotgun.SetActive(!state);
        noClipSword.SetActive(state);
    }

    private void RefreshWalls()
    {
        originalMaterials.Clear(); // Reset the dictionary in case of reload
        walls = GameObject.FindGameObjectsWithTag("NoClipWall");
        foreach (GameObject wall in walls)
        {
            BoxCollider Col = wall.GetComponent<BoxCollider>();
            if (Col != null) {
                Col.isTrigger = false;
            }

            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null) {
                originalMaterials[renderer] = renderer.material;
            }
        }
    }

    public void Activate() {
        noClipStartPosition = transform.position;
        NoClipToggle(true);
        controller.enabled = false;
        foreach (var pair in originalMaterials)
        {
            StartCoroutine(FadeWallsToTransparent(pair.Key, pair.Value));
        }
    }
    public void Deactivate() {
        if (isInNullZone) {
            Debug.Log("Exited NoClip in restricted zone");
            StartCoroutine(RevertPosition(noClipStartPosition));
        }
        NoClipToggle(false);
        controller.enabled = true;
        foreach (var pair in originalMaterials)
        {
            StartCoroutine(FadeWallsToOpaque(pair.Key, pair.Value));
        }
    }

    private void MakeWallsTransparent() {
        foreach (var pair in originalMaterials)
        {
            pair.Key.material = transparentMaterial;
        }
    }
    private void RestoreWalls() {
        foreach (var pair in originalMaterials)
        {
            pair.Key.material = pair.Value;
        }
    }

    private IEnumerator RevertPosition(Vector3 noClipStartPosition) {
        Debug.Log("Reverting Position");
        //transform.position = Vector3.MoveTowards(transform.position, noClipStartPosition, 1 * Time.deltaTime);
        transform.position = noClipStartPosition;
        yield return null;
    }
    private IEnumerator FadeWallsToTransparent(Renderer renderer, Material originalMaterial) {
        // Create a new transparent material for this wall
        Material fadeMat = new Material(transparentMaterial);
        renderer.material = fadeMat;

        float timer = 0f;
        Color color = fadeMat.color;
        float startAlpha = 1f;
        float targetAlpha = 0.5f;

        while (timer < fadeDuration) {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            color.a = alpha;
            fadeMat.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        fadeMat.color = color;
    }
    private IEnumerator FadeWallsToOpaque(Renderer renderer, Material originalMaterial) {
        Material fadeMat = renderer.material;

        float timer = 0f;
        Color color = fadeMat.color;
        float startAlpha = color.a;
        float targetAlpha = 1f;

        while (timer < fadeDuration) {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            color.a = alpha;
            fadeMat.color = color;
            yield return null;
        }

        // Restore the original opaque material
        renderer.material = originalMaterial;
    }
}
