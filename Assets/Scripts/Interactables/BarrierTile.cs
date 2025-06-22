using UnityEngine;

public class BarrierTile : InteractableTile
{
    [SerializeField]
    private float barrierTime = 0.5f;
    
    public override void Interact()
    {
        GameManager.Instance.Player.DoInvulnerable(barrierTime);
        _vfxEvent.Raise();
        gameObject.SetActive(false);
    }

    public override void WalkedOver()
    {
        gameObject.SetActive(false);
    }
}