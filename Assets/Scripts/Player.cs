using UnityEngine;

public class Player : MonoBehaviour
{
    public OrbRotater orbRotater;
    public PlayerMovement playerMovement;

    public void SlowMovement(float slowAmount)
    {
        playerMovement.Slow(slowAmount);
    }
    
    public void Stun()
    {
        StartCoroutine(playerMovement.Stun(2));
    }
    
    public void Boost(float boostAmount)
    {
        playerMovement.Boost(boostAmount);
    }
}