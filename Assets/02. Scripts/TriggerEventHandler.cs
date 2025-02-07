using UnityEngine;
using UnityEngine.Events;

public class TriggerEventHandler : MonoBehaviour
{
    public event UnityAction<Collider> OnEnter;
    public event UnityAction<Collider> OnStay;
    public event UnityAction<Collider> OnExit;

    private void OnTriggerEnter(Collider other)
    {
        OnEnter?.Invoke(other);
    }
    private void OnTriggerStay(Collider other)
    {
        OnStay?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        OnExit?.Invoke(other);
    }
}