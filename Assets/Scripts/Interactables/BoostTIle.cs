using UnityEngine;

public class BoostTIle : InteractableTile
{
    [SerializeField]
    private float _boostAmount = 0.5f;
    
    public override void Interact()
    {
        AudioManager.Instance.PlaySFX("WAV_GJLSpringJam2025_INT_BoostV2");
        GameManager.Instance.Player.Boost(_boostAmount); // Add VFX for speed boost like climbing lines
        _vfxEvent.Raise();
        gameObject.SetActive(false);
    }

    public override void WalkedOver()
    {
        gameObject.SetActive(false);
    }
}
