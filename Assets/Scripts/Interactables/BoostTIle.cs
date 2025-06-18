using UnityEngine;

public class BoostTIle : InteractableTile
{
    [SerializeField]
    private float _boostAmount = 0.5f;
    
    public override void Interact()
    {
        Debug.Log("boosting");
        GameManager.Instance.Player.Boost(_boostAmount);
        gameObject.SetActive(false);
    }

    public override void WalkedOver()
    {
        gameObject.SetActive(false);
    }
}
