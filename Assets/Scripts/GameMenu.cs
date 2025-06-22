using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public CanvasGroup menu;
    public KeyCode menuKey = KeyCode.Escape;
    private bool isShowingMenu = false;
    
    public TMP_Dropdown difficultyDropdown;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    
    public AudioMixer audioMixer;

    void Start()
    {
        ToggleVisibility();
        difficultyDropdown?.onValueChanged.AddListener(OnDifficultyChanged);
        
        masterVolumeSlider?.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider?.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (audioMixer.GetFloat("MasterVolume", out var masterVolume))
        {
            masterVolumeSlider.value = masterVolume;
        }

        if (audioMixer.GetFloat("MusicVolume", out var musicVolume))
        {
            musicVolumeSlider.value = musicVolume;
        }
        
        if (audioMixer.GetFloat("SFXVolume", out var SFXvolume))
        {
            sfxVolumeSlider.value = SFXvolume;
        }
    }

    private void OnDisable()
    {
        difficultyDropdown?.onValueChanged.RemoveListener(OnDifficultyChanged);
        masterVolumeSlider?.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        musicVolumeSlider?.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        sfxVolumeSlider?.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }

    void Update()
    {
        if (Input.GetKeyDown(menuKey))
        {
            ShowMenu();
        }
    }

    void ShowMenu()
    {
        isShowingMenu = !isShowingMenu;
        ToggleVisibility();
    }

    void ToggleVisibility()
    {
        menu.alpha = isShowingMenu ? 1f : 0f;
        menu.interactable = isShowingMenu;
        menu.blocksRaycasts = isShowingMenu;
        Time.timeScale = isShowingMenu ? 0f : 1f;
    }

    public void Resume()
    {
        ShowMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnDifficultyChanged(int difficulty)
    {
        
    }

    public void OnMasterVolumeChanged(float db)
    {
        
        audioMixer.SetFloat("MasterVolume", db);
    }
    
    public void OnMusicVolumeChanged(float db)
    {
        audioMixer.SetFloat("MusicVolume", db);
    }
    
    public void OnSFXVolumeChanged(float db)
    {
        audioMixer.SetFloat("SFXVolume", db);
    }
}
