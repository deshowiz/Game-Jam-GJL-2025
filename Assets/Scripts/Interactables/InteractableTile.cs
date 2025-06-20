using System;
using UnityEngine;

public class InteractableTile : Tile
{
    public bool used = false;

    [SerializeField]
    protected GameEvent _vfxEvent = null;

    public virtual void Interact() {}
    public virtual void WalkedOver() {}
}
