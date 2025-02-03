using UnityEngine;

public class PCController : MonoBehaviour
{
    [SerializeField] private CharacterController _controller;
    [SerializeField] private float _speed;

    private void Update()
    {
        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");
        var moveDir = new Vector3(h, 0, v)  * _speed * Time.deltaTime;
        Debug.Log($"PCC : {_controller.velocity}");
        _controller.Move(moveDir);
    }
}
