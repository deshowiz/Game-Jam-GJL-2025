using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    public Image image;
    public float fadeDuration = 1f;
    void Start()
    {
        StartFadeOut();
    }
    
    public void StartFadeIn()
    {
        StartCoroutine(FadeIn());
    }
    
    public void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }
    
    IEnumerator FadeIn()
    {
        float t = 0f;
        Color color = image.color;
        
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            image.color = color;
            yield return null;
        }
        
        color.a = 1f;
        image.color = color;
    }
    
    IEnumerator FadeOut()
    {
        float t = 0f;
        Color color = image.color;
        
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            image.color = color;
            yield return null;
        }
        
        color.a = 0f;
        image.color = color;
    }
}