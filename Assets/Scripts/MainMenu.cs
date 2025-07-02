using System;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        GameObject audioManager = Instantiate(Resources.Load<GameObject>("AudioManager"), Vector3.zero, Quaternion.identity);
    }

    public void PlaySingle()
    {
        SceneManager.LoadScene(2);
    }

    public void PlayDouble()
    {
        SceneManager.LoadScene(3);
    }

    public void PlayTurbo()
    {
        SceneManager.LoadScene(4);
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}
