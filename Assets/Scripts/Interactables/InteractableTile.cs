using System;
using UnityEngine;

public class InteractableTile : Tile
{
    public bool used = false;

    public virtual void Interact() { }
    public virtual void WalkedOver() {}
}
