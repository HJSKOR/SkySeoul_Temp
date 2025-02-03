using UnityEngine;

public class PCAnimatorController : MonoBehaviour
{
    [SerializeField] public Animator Animator;
    [SerializeField] private CharacterController _controller;
    private float _canShootingTime;
    private void Awake()
    {
        Animator.applyRootMotion = false;
    }
    private void Update()
    {
        var h = _controller.velocity.x;
        var v = _controller.velocity.z;
        Animator.SetFloat("Horizontal", h);
        Animator.SetFloat("Vertical", v);
        if (Input.GetKey(KeyCode.C))
        {
            Crouch();
        }
        else
        {
            EndCrouch();
        }

        if (Input.GetKey(KeyCode.Mouse0) && _canShootingTime < Time.time)
        {
            _canShootingTime = Time.time + 0.5f;
            Fire();
        }
        if(_canShootingTime < Time.time)
        {
            FireEnd();
        }
    }

    private void Walk()
    {

    }

    private void Run()
    {
        Animator.SetInteger("BodyState", 4);
    }

    private void Hit()
    {

    }
    private void Fire()
    {
        Animator.SetInteger("UpperState", 1);
    }
    private void FireEnd()
    {
        Animator.SetInteger("UpperState", 0);
    }
    private void Crouch()
    {
        Animator.SetInteger("BodyState", 3);
    }
    private void EndCrouch()
    {
        Animator.SetInteger("BodyStae", 3);
    }
    private void Idle()
    {
        Animator.SetFloat("BodyState", 0);
    }
}
