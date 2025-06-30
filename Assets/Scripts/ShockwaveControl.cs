using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ShockwaveControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject _effectPlane = null;
    [SerializeField]
    private Material _shockwaveMaterial = null;
    [SerializeField]
    private Material _playerMaterial = null;
    [SerializeField]
    private Material _minotaurMaterial = null;
    [Header("Settings")]
    [SerializeField]
    private float _shockwaveMaxProgress = 0.7f;
    [SerializeField]
    private float _shockwaveSpeed = 1f;
    [SerializeField]
    [ColorUsage(false, true)]
    private Color _shockwaveColorMult = Color.white;
    private Coroutine _shockwaveRoutine = null;
    public void InitiateShockwave()
    {
        if (_shockwaveRoutine != null)
        {
            StopCoroutine(_shockwaveRoutine);
        }
        _shockwaveRoutine = StartCoroutine(Shockwaving());
    }

    private IEnumerator Shockwaving()
    {
        _shockwaveMaterial.SetFloat("_Progress", 0f);
        _effectPlane.SetActive(true);
        _playerMaterial.SetColor("_ShockwaveTInt", _shockwaveColorMult);
        _minotaurMaterial.SetColor("_ShockwaveTInt", _shockwaveColorMult);
        float currentProgress = 0f;
        float startTime = Time.time;
        while (currentProgress < 1f)
        {
            currentProgress = (Time.time - startTime) * _shockwaveSpeed;
            float fullProgress = Mathf.Lerp(-0.1f, _shockwaveMaxProgress, currentProgress);
            _shockwaveMaterial.SetFloat("_Progress", fullProgress);
            yield return null;
        }
    }

    public void RemoveFilter()
    {
        _playerMaterial.SetColor("_ShockwaveTInt", Color.white);
        _minotaurMaterial.SetColor("_ShockwaveTInt", Color.white);
        _effectPlane.SetActive(false);
    }

    private void OnDisable()
    {
        _playerMaterial.SetColor("_ShockwaveTInt", Color.white);
        _minotaurMaterial.SetColor("_ShockwaveTInt", Color.white);
    }
}
