using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private SphereCollider _range;
    [SerializeField] private string _targetTage;
    [SerializeField] private Vector3 _force;

    private void Awake()
    {
        _range.isTrigger = true;
        _range.enabled = false;
    }
    public void DoExplosoin()
    {
        _range.enabled = false;
        _range.enabled = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("targetTage") && !string.IsNullOrEmpty(_targetTage))
        {
            return;
        }
        if (other.TryGetComponent<Rigidbody>(out var rigid))
        {
            rigid.isKinematic = false;
            var dir = (other.transform.position - transform.position).normalized;

            rigid.AddForceAtPosition(Vector3.Scale(_force * _range.radius, dir), transform.position, ForceMode.Impulse);
        }
    }
}
