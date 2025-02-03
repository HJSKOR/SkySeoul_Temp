using UnityEngine;

public abstract class bl_PlayerAnimationsBase : MonoBehaviour
{
    [SerializeField] private Animator m_animator = null;
    public Animator Animator
    {
        get => m_animator;
        set => m_animator = value;
    }
    public PlayerState BodyState
    {
        get;
        set;
    } = PlayerState.Idle;
    public PlayerFPState FPState
    {
        get;
        set;
    } = PlayerFPState.Idle;
    public bool IsGrounded
    {
        get;
        set;
    }
    public Vector3 Velocity
    {
        get;
        set;
    } = Vector3.zero;
    public Vector3 LocalVelocity
    {
        get;
        set;
    } = Vector3.zero;
    public abstract void SetNetworkGun(GunType weaponType, bl_NetworkGun networkGun);
    public abstract void PlayFireAnimation(GunType gunType);
    public abstract void UpdateAnimatorParameters();
    public abstract void OnGetHit();
    public abstract void BlockWeapons(int blockType);
}