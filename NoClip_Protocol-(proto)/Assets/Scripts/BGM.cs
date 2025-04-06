using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGM : MonoBehaviour
{
    public static BGM instance;
    private AudioSource audioSource;
    public List<AudioClip> AudioClips;
    // "ODDVIBE - Hatchback Racer"
    // "Kaixo - Mystic"
    private Dictionary<String, AudioClip> clipDictionary;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        clipDictionary = new Dictionary<string, AudioClip>();
        foreach (var clip in AudioClips) {
            clipDictionary[clip.name] = clip; // Store using the clip's name
        }
        SetMusic("ODDVIBE - Hatchback Racer");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep music playing across scenes
        }
        else {
            Destroy(gameObject); // Prevent duplicates
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void SetMusic(string clipName)
    {
        if (clipDictionary.TryGetValue(clipName, out AudioClip newClip)) {
            audioSource.Stop();
            audioSource.clip = newClip;
            audioSource.Play();
        }
        else {
            Debug.LogWarning($"Audio clip '{clipName}' not found!");
        }
    }

    public void FadeOut(float duration) {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private IEnumerator FadeOutCoroutine(float duration) {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration) {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        audioSource.Stop(); 
    }
    
}
