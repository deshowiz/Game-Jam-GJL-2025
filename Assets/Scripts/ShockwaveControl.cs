using System.Collections;
using UnityEngine;

public class ShockwaveControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject _effectPlane = null;
    [SerializeField]
    private Material _shockwaveMaterial;
    [Header("Settings")]
    [SerializeField]
    private float _shockwaveMaxProgress = 0.7f;
    [SerializeField]
    private float _shockwaveSpeed = 1f;
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
        _effectPlane.SetActive(false);
    }
}
