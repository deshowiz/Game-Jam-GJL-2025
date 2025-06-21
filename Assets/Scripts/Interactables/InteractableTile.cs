using System;
using UnityEngine;

public class InteractableTile : Tile
{
    public bool used = false;
    public int _listIndex = 0;

    [SerializeField]
    protected GameEvent _vfxEvent = null;

    public virtual void Interact() {}
    public virtual void WalkedOver() {}
}
