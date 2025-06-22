using System;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int Direction = Animator.StringToHash("Direction");
    public Animator animator;
    public PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        Vector3 dir = playerMovement.GetNormalizedDirection();
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
        else if (dir.x > 0)
        {
            direction = CharacterDirection.Right;
        }
        else if (dir.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        animator.SetInteger(Direction, (int)direction);
        
        //Debug.Log(playerMovement.GetNormalizedDirection() + " - Right: " + right);
    }
}

public enum CharacterDirection
{
    Right = 0,
    Up = 1,
    Down = 2
}