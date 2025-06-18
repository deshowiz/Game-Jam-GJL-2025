using UnityEngine;

public class StunTile : InteractableTile
{
    [SerializeField]
    private float _stunTime = 0.5f;
    [SerializeField]
    private float _adjustedTimeScale = 0.3f;

    private bool _isArmed = false;

    public override void Interact()
    {
        gameObject.SetActive(false);
    }


    public override void WalkedOver()
    {
        if (_isArmed)
        {
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
