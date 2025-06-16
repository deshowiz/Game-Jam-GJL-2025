using UnityEngine;

public class Player : MonoBehaviour
{
    public OrbRotater orbRotater;
    public PlayerMovement playerMovement;

    public void SlowMovement()
    {
        playerMovement.Slow(10);
    }
    
    public void Stun()
    {
        StartCoroutine(playerMovement.Stun(2));
    }
    
    public void Boost()
    {
        playerMovement.Boost(15);
    }
}