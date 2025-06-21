using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;


public class UIContinueButton : MonoBehaviour
{
    public void Continue()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
