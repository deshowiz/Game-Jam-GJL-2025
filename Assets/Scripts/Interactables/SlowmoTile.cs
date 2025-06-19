using UnityEngine;

public class SlowmoTile : StunTile
{
    [SerializeField]
    private int _numPressesforUnlock = 20;
    public override void WalkedOver()
    {
        if (_isArmed)
        {
            Time.timeScale = _adjustedTimeScale;
            GameManager.Instance.Player.SlowMo(_numPressesforUnlock);
        }
    }
}
