using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class A : MonoBehaviour
{
    private CharacterController _controller;
    void Start()
    {
        _controller = GetComponent<CharacterController>();   
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 velocity = Vector3.zero;
        velocity.x = Input.GetAxis("Horizontal");
        velocity.z = Input.GetAxis("Vertical");
        _controller.Move(velocity);
    }
}
