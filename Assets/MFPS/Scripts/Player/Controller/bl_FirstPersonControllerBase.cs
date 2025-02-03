using UnityEngine;
using MFPS.PlayerController;


public abstract class bl_FirstPersonControllerBase : MonoBehaviour
{
    public PlayerState State 
    {
        get; 
        set; 
    } = PlayerState.Idle;
    public abstract Vector3 Velocity
    {
        get;
        set;
    }
    public abstract float VelocityMagnitude
    {
        get; 
        set;
    }
    public abstract bool isControlable
    {
        get;
        set;
    }
    public abstract bool isGrounded 
    { 
        get;
    }
    private bl_FirstPersonController _mfpsController = null;
    public bl_FirstPersonController MFPSController
    {
        get
        {
            if (_mfpsController == null) TryGetComponent(out _mfpsController);
            return _mfpsController;
        }
    }
    public abstract void DoJump();
    public abstract void DoSlide();
    public abstract void DoDrop();
    public abstract void DoGliding();
    public abstract void Stop();
    public abstract void PlatformJump(float force);
    public abstract void UpdateMouseLook();
    public abstract void PlayFootStepSound();
    public abstract MouseLookBase GetMouseLook();
    public abstract bl_Footstep GetFootStep();
    public abstract float GetSpeedOnState(PlayerState playerState, bool includeModifiers = false);
    public abstract float GetCurrentSpeed();
    public abstract PlayerRunToAimBehave GetRunToAimBehave();
    public abstract float GetHeadBobMagnitudes(bool verticalMagnitude);
    public abstract float GetSprintFov();
}