using Unity.VisualScripting;
using UnityEngine;

public class SpikeTrap : InteractableTile
{
    [SerializeField]
    private float _slowdownAmount = 0.5f;

    private bool _isArmed = false;

    public override void Interact()
    {
        gameObject.SetActive(false);
    }


    public override void WalkedOver()
    {
        if (_isArmed)
        {
            GameManager.Instance.Player.SlowMovement(_slowdownAmount);
            // Add spike trap anim here
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
