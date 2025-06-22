using System;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int Direction = Animator.StringToHash("Direction");
    public Animator animator;
    public PlayerMovement playerMovement;
    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Vector3 dir = playerMovement.GetNormalizedDirection();
        CharacterDirection direction = CharacterDirection.Right;
        
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