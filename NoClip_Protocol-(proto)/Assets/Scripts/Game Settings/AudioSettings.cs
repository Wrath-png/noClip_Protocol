using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider masterSlider, musicSlider, sfxSlider, voiceSlider;
    private float mBaseVol, sBaseVol, vBaseVol, masterVol;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Load saved volume values or set default to 0dB
        masterVol = PlayerPrefs.GetFloat("MasterVolume", 0f);
        mBaseVol = PlayerPrefs.GetFloat("MusicBase", 0f);
        sBaseVol = PlayerPrefs.GetFloat("SFXBase", 0f);
        vBaseVol = PlayerPrefs.GetFloat("VoiceBase", 0f);

        masterSlider.value = masterVol;
        musicSlider.value = mBaseVol;
        sfxSlider.value = sBaseVol;
        voiceSlider.value = vBaseVol;

        ApplyVolumes();
    }

    public void SetMasterVolume(float value)
    {
        masterVol = value;
        PlayerPrefs.SetFloat("MasterVolume", masterVol);
        ApplyVolumes();
    }

    public void SetMusicVolume(float value)
    {
        mBaseVol = value;
        PlayerPrefs.SetFloat("MusicBase", mBaseVol);
        ApplyVolumes();
    }

    public void SetSFXVolume(float value)
    {
        sBaseVol = value;
        PlayerPrefs.SetFloat("SFXBase", sBaseVol);
        ApplyVolumes();
    }

    public void SetVoiceVolume(float value)
    {
        vBaseVol = value;
        PlayerPrefs.SetFloat("VoiceBase", vBaseVol);
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        //Debug.Log($"Applying Volumes: Music={mBaseVol + masterVol}, SFX={sBaseVol + masterVol}, Voice={vBaseVol + masterVol}");
        mixer.SetFloat("MusicBase", mBaseVol + masterVol);
        mixer.SetFloat("SFXBase", sBaseVol + masterVol);
        mixer.SetFloat("VoiceBase", vBaseVol + masterVol);
    }
}
