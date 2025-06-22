using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool invulnerable;
    public OrbRotater orbRotater;
    public PlayerMovement playerMovement;

    public void SlowMovement(float slowAmount)
    {
        if (invulnerable) return;
        playerMovement.Slow(slowAmount);
    }
    
    public void Stun(float stunDuration)
    {
        if (invulnerable) return;
        playerMovement.Stun(stunDuration);
    }

    public void SlowMo(int numPresses)
    {
        if (invulnerable) return;
        playerMovement.SlowMo(numPresses);
    }
    
    public void Boost(float boostAmount)
    {
        playerMovement.Boost(boostAmount);
    }

    public void DoInvulnerable(float duration)
    {
        StartCoroutine(InvulnerableRoutine(duration));
    }

    IEnumerator InvulnerableRoutine(float duration)
    {
        invulnerable = true;
        yield return new WaitForSeconds(duration);
        invulnerable = false;
    }
}