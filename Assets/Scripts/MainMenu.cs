using System;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        GameObject audioManager = Instantiate(Resources.Load<GameObject>("AudioManager"), Vector3.zero, Quaternion.identity);
    }

    public void Play()
    {
        SceneManager.LoadScene(2);
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}
