using UnityEngine;

public class Player : MonoBehaviour
{
    public OrbRotater orbRotater;
    public PlayerMovement playerMovement;

    public void SlowMovement(float slowAmount)
    {
        playerMovement.Slow(slowAmount);
    }
    
    public void Stun(float stunDuration)
    {
        playerMovement.Stun(stunDuration);
    }

    public void SlowMo(int numPresses)
    {
        playerMovement.SlowMo(numPresses);
    }
    
    public void Boost(float boostAmount)
    {
        playerMovement.Boost(boostAmount);
    }
}