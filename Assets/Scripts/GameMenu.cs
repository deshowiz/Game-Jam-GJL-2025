using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public CanvasGroup menu;
    public KeyCode menuKey = KeyCode.Escape;
    private bool isShowingMenu = false;
    
    public TMP_Dropdown difficultyDropdown;

    void Start()
    {
        ToggleVisibility();
        difficultyDropdown?.onValueChanged.AddListener(OnDifficultyChanged);
    }

    private void OnDisable()
    {
        difficultyDropdown?.onValueChanged.RemoveListener(OnDifficultyChanged);
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
}
