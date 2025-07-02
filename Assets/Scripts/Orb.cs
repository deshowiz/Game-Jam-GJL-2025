using System.Collections;
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
    [SerializeField]
    private float _emissionAdditive = 1f;
    [SerializeField]
    private float _emissionSpeed = 0.3f;
    [SerializeField]
    private float _baseEmissiveLevel = 5.9f;
    private bool _isDisabled = false;
    public bool IsDisabled {get{ return _isDisabled; }}
    Coroutine _glowRoutine = null;
    private bool _isRecovering = false;

    public async void DisableOrb()
    {
        if (_isDisabled) return;
        _isDisabled = true;
        if (_glowRoutine != null)
        {
            StopCoroutine(_glowRoutine);
        }
        Color enabledColor = _orbMat.GetColor("_OrbColor");
        _orbMat.SetColor("_OrbColor", Color.black);
        _orbMat.SetFloat("_EmissionStrength", 1f);
        await Task.Delay(_missDisableTimeMilliseconds);
        if (_isRecovering) return;
        _orbMat.SetFloat("_EmissionStrength", _baseEmissiveLevel);
        _orbMat.SetColor("_OrbColor", enabledColor);
        _isDisabled = false;
    }

    public void FullDisable()
    {
        _isRecovering = true;
        _isDisabled = true;
        _orbMat.SetFloat("_EmissionStrength", 1f);
        _orbMat.SetColor("_OrbColor", Color.black);
    }

    public void ReEnable()
    {
        _isDisabled = false;
        _isRecovering = false;
    }

    public bool SetOrbRecovery(float progressPercentage)
    {
        _orbMat.SetColor("_OrbColor", Color.Lerp(Color.black, _usualColor, progressPercentage));
        _orbMat.SetFloat("_EmissionStrength", Mathf.Lerp(1f, _baseEmissiveLevel, progressPercentage));
        return true;
    }

    public void GlowOnHit()
    {
        if (_glowRoutine != null)
        {
            StopCoroutine(_glowRoutine);
        }
        _glowRoutine = StartCoroutine(Glow());
    }

    public void HeldGlow()
    {
        _orbMat.SetFloat("_EmissionStrength", _baseEmissiveLevel + _emissionAdditive / 2f);
    }
    public void UnHeldGlow()
    {
        _orbMat.SetFloat("_EmissionStrength", _baseEmissiveLevel);
    }

    private IEnumerator Glow()
    {
        float endTime = Time.time + _emissionSpeed;
        while (Time.time < endTime)
        {
            if (_isDisabled) break;
            yield return null;
            _orbMat.SetFloat("_EmissionStrength", Mathf.Lerp(_baseEmissiveLevel,
            _baseEmissiveLevel + _emissionAdditive, (endTime - Time.time) / _emissionSpeed));
        }
    }

    public void OnDisable()
    {
        _orbMat.SetColor("_OrbColor", _usualColor);
        _orbMat.SetFloat("_EmissionStrength", _baseEmissiveLevel);
    }
}
