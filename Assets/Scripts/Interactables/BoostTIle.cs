using UnityEngine;

public class BoostTIle : InteractableTile
{
    [SerializeField]
    private float _boostAmount = 0.5f;
    
    public override void Interact()
    {
        GameManager.Instance.Player.Boost(_boostAmount); // Add VFX for speed boost like climbing lines
        gameObject.SetActive(false);
    }

    public override void WalkedOver()
    {
        gameObject.SetActive(false);
    }
}
