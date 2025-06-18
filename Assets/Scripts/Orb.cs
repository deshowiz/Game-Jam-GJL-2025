using System.Threading.Tasks;
using UnityEngine;

public class Orb : MonoBehaviour
{
    [SerializeField]
    private Material _orbMat = null;
    [SerializeField]
    [Range(0, 5000)]
    private int _missDisableTimeMilliseconds = 300;
    [SerializeField]
    private Color _usualColor = Color.green;

    private bool _isDisabled = false;
    public bool IsDisabled {get{ return _isDisabled; }}

    public async void DisableOrb()
    {
        if (_isDisabled) return;
        _isDisabled = true;
        Color enabledColor = _orbMat.GetColor("_OrbColor");
        _orbMat.SetColor("_OrbColor", Color.gray);
        await Task.Delay(_missDisableTimeMilliseconds);
        _orbMat.SetColor("_OrbColor", enabledColor);
        _isDisabled = false;
    }

    public void OnDisable()
    {
        _orbMat.SetColor("_OrbColor", _usualColor);
    }
}
