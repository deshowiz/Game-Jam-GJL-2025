using System;
using UnityEngine;

public class InteractableTile : Tile
{
    public bool used = false;
    public InteractableType interactableType;
    
    public void Interact(Player player, bool success)
    {
        if (used) return;
        
        switch (interactableType)
        {
            case InteractableType.Spikes:
                if (!success)
                {
                    player.SlowMovement();
                }
                break;
            case InteractableType.Enemy:
                if (!success)
                {
                    player.Stun();
                }
                break;
            case InteractableType.Boost:
                if (success)
                {
                    player.Boost();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        used = true;
    }
}

public enum InteractableType
{
    Spikes,
    Enemy,
    Boost
}
