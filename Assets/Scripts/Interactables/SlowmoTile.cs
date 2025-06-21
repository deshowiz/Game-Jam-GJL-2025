using UnityEngine;

public class SlowmoTile : StunTile
{
    [SerializeField]
    private GameEvent _shockwaveEvent = null;
    [SerializeField]
    private int _numPressesforUnlock = 20;
    public override void WalkedOver()
    {
        if (_isArmed)
        {
            Time.timeScale = _adjustedTimeScale;
            _shockwaveEvent.Raise();
            GameManager.Instance.Player.SlowMo(_numPressesforUnlock);
        }
    }
}
