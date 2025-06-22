using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MinotaurAnimation : MonoBehaviour
{
    public float minHuff = 3f;
    public float maxHuff = 5f;
    private float nextHuff = 0f;

    private void Start()
    {
        HuffTimer();
    }

    public void Footstep()
    {
        int rand = Random.Range(1, 10);
        string number = rand.ToString("D2");
        string clipName = $"WAV_GJLSpringJam2025_FS_Minotaur_Stone_{number}";

        AudioManager.Instance.PlaySFX(clipName);
    }
    
    public void Huff()
    {
        int rand = Random.Range(1, 12);
        string number = rand.ToString("D2");
        string clipName = $"WAV_GJLSpringJam2025_VOCAL_Minotaur_Huff_{number}";

        AudioManager.Instance.PlaySFX(clipName);
        HuffTimer();
    }

    private void HuffTimer()
    {
        float delay = Random.Range(minHuff, maxHuff);
        nextHuff = Time.time + delay;
    }
    
    private void Update()
    {
        if (Time.time >= nextHuff)
        {
            Huff();
        }
    }
}
