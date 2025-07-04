using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    public GameEvent Event;
    public UnityEvent Response;

    public void OnEnable()
    {
        Event?.RegisterListener(this);
    }

    public void OnDisable()
    {
        Event?.UnRegisterListener(this);
    }

    public void OnEventRaised()
    {
        Response?.Invoke();
    }
}
