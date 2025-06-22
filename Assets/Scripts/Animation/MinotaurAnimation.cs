using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MinotaurAnimation : MonoBehaviour
{
    public float minHuff = 3f;
    public float maxHuff = 5f;
    private float nextHuff = 0f;
    
    private static readonly int Direction = Animator.StringToHash("Direction");
    public Animator animator;
    public MinotaurMovement movement;
    
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        HuffTimer();
        
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<MinotaurMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Footstep()
    {
        if (movement.LinearDistanceToPlayer() > 13)
        {
            return;
        }
        int rand = Random.Range(1, 10);
        string number = rand.ToString("D2");
        string clipName = $"WAV_GJLSpringJam2025_FS_Minotaur_Stone_{number}";

        AudioManager.Instance.PlaySFX(clipName);
    }
    
    public void Huff()
    {
        if (movement.LinearDistanceToPlayer() > 13)
        {
            return;
        }
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
        
        Vector3 dir = movement.GetNormalizedDirection();
        CharacterDirection direction = CharacterDirection.Right;
        spriteRenderer.flipX = false;
        
        if (dir.z > 0.8f && dir.x <= 0)
        {
            direction = CharacterDirection.Up;
        }
        else if (dir.z < -0.8f && dir.x <= 0)
        {
            direction = CharacterDirection.Down;
        }
        else if (dir.x < 0)
        {
            direction = CharacterDirection.Right;
        }
        else if (dir.x > 0)
        {
            spriteRenderer.flipX = true;
        }
        animator.SetInteger(Direction, (int)direction);
    }
}
