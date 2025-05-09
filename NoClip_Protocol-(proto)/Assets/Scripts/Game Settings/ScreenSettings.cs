using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class ScreenSettings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;

    void Start()
    {
        resolutions = Screen.resolutions;
        for(int i = 0; i < resolutions.Length; i++) {
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(resolutions[i].ToString()));
        }

        Resolution currentResolution = Screen.currentResolution;

        int currentIndex = PlayerPrefs.GetInt("resolution", -1);
        if (currentIndex == -1) {
            currentIndex = Array.IndexOf(resolutions, currentResolution);
        }

        resolutionDropdown.value = Array.IndexOf(resolutions, currentResolution);
    }

    public void SetResolution() {
        int currentIndex = resolutionDropdown.value;
        Resolution rez = resolutions[currentIndex];
        Screen.SetResolution(rez.width, rez.height, Screen.fullScreen);
        PlayerPrefs.SetInt("resolution", currentIndex);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
