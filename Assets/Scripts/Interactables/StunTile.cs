using UnityEngine;

public class StunTile : InteractableTile
{
    [SerializeField]
    private float _stunTime = 0.5f;
    [SerializeField]
    protected float _adjustedTimeScale = 0.3f;

    protected bool _isArmed = false;

    public override void Interact()
    {
        gameObject.SetActive(false);
    }


    public override void WalkedOver()
    {
        if (_isArmed)
        {
            _vfxEvent.Raise();
            AudioManager.Instance.PlaySFX("WAV_GJLSpringJam2025_INT_Stun");
            AudioManager.Instance.PlaySFX("WAV_GJLSpringJam2025_INT_Stun_BirdsChirping");
            Time.timeScale = _adjustedTimeScale;
            GameManager.Instance.Player.Stun(_stunTime);
        }
    }

    private void OnDisable()
    {
        _isArmed = false;
    }

    private void OnEnable()
    {
        _isArmed = true;
    }
}
