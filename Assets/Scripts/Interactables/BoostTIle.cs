using UnityEngine;

public class BoostTIle : InteractableTile
{
    [SerializeField]
    private float _boostAmount = 0.5f;
    [SerializeField]
    private GameEvent _boostedEvent = null;
    
    public override void Interact()
    {
        GameManager.Instance.Player.Boost(_boostAmount); // Add VFX for speed boost like climbing lines
        _boostedEvent.Raise();
        gameObject.SetActive(false);
    }

    public override void WalkedOver()
    {
        gameObject.SetActive(false);
    }
}
