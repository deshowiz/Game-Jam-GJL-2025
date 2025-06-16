using System;
using UnityEngine;

public class InteractableTile : Tile
{
    public InteractableType interactableType;
    
    public void Interact(Player player, bool success)
    {
        switch (interactableType)
        {
            case InteractableType.Spikes:
                if (success)
                {
                    RemoveTile();
                }
                else
                {
                    player.SlowMovement();
                }
                break;
            case InteractableType.Enemy:
                if (success)
                {
                    RemoveTile();
                }
                else
                {
                    player.Stun();
                }
                break;
            case InteractableType.Boost:
                if (success)
                {
                    RemoveTile();
                }
                else
                {
                    player.Boost();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void RemoveTile()
    {
        //how to correctly remove tile from tilegenerator, also should be a Tile method
    }
}

public enum InteractableType
{
    Spikes,
    Enemy,
    Boost
}
