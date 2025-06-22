using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIMinotaurBar : MonoBehaviour
{
    public float maxDistance = 20;
    public Slider slider;
    
    private float targetValue;
    private bool isLerping = false;

    [SerializeField]
    private AnimationCurve _distanceCurve = new AnimationCurve();
    
    public void UpdateBar(float distance)
    {
        float percent = (1f - _distanceCurve.Evaluate(distance / maxDistance)) * 100;
        percent = Mathf.Clamp(percent, 0, 100);

        targetValue = percent;
        isLerping = true;
    }

    private void Update()
    {
        if (isLerping)
        {
            slider.value = Mathf.Lerp(slider.value, targetValue, Time.deltaTime * 2f);
        
            if (Mathf.Abs(slider.value - targetValue) < 0.1f)
            {
                slider.value = targetValue;
                isLerping = false;
            }
        }
    }
}
