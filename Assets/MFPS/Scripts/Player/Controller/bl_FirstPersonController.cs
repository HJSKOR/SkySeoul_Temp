using UnityEngine;
using System.Collections;
using MFPS.PlayerController;
using UnityEngine.Serialization;
using MFPS.Runtime.Level;
using System;

[RequireComponent(typeof(CharacterController))]
public class bl_FirstPersonController : bl_FirstPersonControllerBase
{
    #region Public members
    [Header("Settings")]
    public float WalkSpeed = 4.5f;
    [FormerlySerializedAs("m_CrouchSpeed")]
    public float runSpeed = 8;
    public float stealthSpeed = 1;
    [FormerlySerializedAs("m_CrouchSpeed")]
    public float crouchSpeed = 2;
    public float slideSpeed = 10;
    [FormerlySerializedAs("m_ClimbSpeed")]
    public float climbSpeed = 1f;
    [FormerlySerializedAs("m_JumpSpeed")]
    public float jumpSpeed;
    public float acceleration = 9;
    public float slopeFriction = 3f;

    public float crouchTransitionSpeed = 0.25f;
    public float crouchHeight = 1.4f;

    public bool canSlide = true;
    [Range(0.2f, 1.5f)] public float slideTime = 0.75f;
    [Range(1, 12)] public float slideFriction = 10;
    [Range(0.1f, 2.5f)] public float slideCoolDown = 1.2f;
    public float slideCameraTiltAngle = -22;

    [Range(0, 2)] public float JumpMinRate = 0.82f;
    public float jumpMomentumBooster = 2;
    public float momentunDecaySpeed = 5;
    [Range(0, 2)] public float AirControlMultiplier = 0.8f;
    public float m_StickToGroundForce;
    public float m_GravityMultiplier;

    [LovattoToogle] public bool RunFovEffect = true;
    public float runFOVAmount = 8;
    [LovattoToogle] public bool KeepToCrouch = true;
    public bool canStealthMode = true;
    [Header("Falling")]
    [LovattoToogle] public bool FallDamage = true;
    [Range(0.1f, 5f)]
    public float SafeFallDistance = 3;
    [Range(3, 25)]
    public float DeathFallDistance = 15;
    public PlayerRunToAimBehave runToAimBehave = PlayerRunToAimBehave.StopRunning;

    [Header("Dropping")]
    public float dropControlSpeed = 25;
    public Vector2 dropTiltSpeedRange = new Vector2(20, 60);
    [Header("Mouse Look"), FormerlySerializedAs("m_MouseLook")]
    public MouseLook mouseLook;
    [FormerlySerializedAs("HeatRoot")]
    public Transform headRoot;
    public Transform CameraRoot;
    [Header("HeadBob")]
    [Range(0, 1.2f)] public float headBobMagnitude = 0.9f;
    public float headVerticalBobMagnitude = 0.4f;
    public LerpControlledBob m_JumpBob = new LerpControlledBob();
    [Header("FootSteps")]
    public bl_Footstep footstep;
    public AudioClip jumpSound;           // the sound played when character leaves the ground.
    public AudioClip landSound;           // the sound played when character touches back on ground.
    public AudioClip slideSound;

    [Header("UI")]
    public Sprite StandIcon;
    public Sprite CrouchIcon;
    #endregion

    #region Public properties
    public float RunFov { get; set; } = 0;
    public CollisionFlags m_CollisionFlags { get; set; }
    public override Vector3 Velocity { get; set; }
    public override float VelocityMagnitude { get; set; }
    public override bool isControlable { get; set; } = true;
    #endregion

    #region Private members
    private bool hasPlatformJump = false;
    private float PlatformJumpForce = 0;
    private bool m_Jump, jumpPressed;
    private Vector2 m_Input = new Vector2();
    private Vector3 targetDirection, moveDirection = Vector3.zero;
    private bool m_PreviouslyGrounded = true;
    private bool m_Jumping = false;
    private bool Crounching = false;
    private AudioSource m_AudioSource;
    private bool Finish = false;
    private Vector3 defaultCameraRPosition;
    private bool isClimbing, isAiming = false;
    private bl_Ladder m_Ladder;
    private bool MoveToStarted = false;
#if MFPSM
    private bl_Joystick Joystick;
#endif
    private float PostGroundVerticalPos = 0;
    private bool isFalling = false;
    private int JumpDirection = 0;
    private float HigherPointOnJump;
    private CharacterController m_CharacterController;
    private float lastJumpTime = 0;
    private float WeaponWeight = 1;
    private bool hasTouchGround = false;
    private bool JumpInmune = false;
    private Transform m_Transform;
    private RaycastHit[] SurfaceRay = new RaycastHit[1];
    private Vector3 desiredMove, momentum = Vector3.zero;
    private float VerticalInput, HorizontalInput;
    private bool lastCrouchState = false;
    private float fallingTime = 0;
    private bool haslanding = false;
    private float capsuleRadious;
    private readonly Vector3 feetPositionOffset = new Vector3(0, 0.8f, 0);
    private float slideForce = 0;
    private float lastSlideTime = 0;
    private bl_PlayerReferences playerReferences;
    private Vector3 forwardVector = Vector3.forward;
    private PlayerState lastState = PlayerState.Idle;
    private bool forcedCrouch = false;
    private Vector3 surfaceNormal, surfacePoint = Vector3.zero;
    private float defaultStepOffset = 0.4f;
    private float desiredSpeed = 4;
    private float defaultHeight = 2;
    private bool overrideNextLandEvent = false;
    #endregion

    #region Unity Methods
    protected void Awake()
    {
        m_Transform = transform;
        playerReferences = GetComponent<bl_PlayerReferences>();
        m_CharacterController = playerReferences.characterController;
        defaultCameraRPosition = CameraRoot.localPosition;
        m_AudioSource = gameObject.AddComponent<AudioSource>();
        mouseLook.Init(m_Transform, headRoot);
        lastJumpTime = Time.time;
        defaultStepOffset = m_CharacterController.stepOffset;
        capsuleRadious = m_CharacterController.radius * 0.5f;
        defaultHeight = m_CharacterController.height;
        isControlable = bl_MatchTimeManagerBase.HaveTimeStarted();
    }
    protected void OnEnable()
    {
        bl_EventHandler.onRoundEnd += OnRoundEnd;
        bl_EventHandler.onChangeWeapon += OnChangeWeapon;
        bl_EventHandler.onMatchStart += OnMatchStart;
        bl_EventHandler.onGameSettingsChange += OnGameSettingsChanged;
        bl_EventHandler.onLocalAimChanged += OnAimChange;
        // TODO
        Finish = false;
    }
    protected void OnDisable()
    {
        bl_EventHandler.onRoundEnd -= OnRoundEnd;
        bl_EventHandler.onChangeWeapon -= OnChangeWeapon;
        bl_EventHandler.onMatchStart -= OnMatchStart;
        bl_EventHandler.onGameSettingsChange -= OnGameSettingsChanged;
        bl_EventHandler.onLocalAimChanged -= OnAimChange;
    }
    public void Update()
    {
        Velocity = m_CharacterController.velocity;
        VelocityMagnitude = Velocity.magnitude;

        if (Finish) return;

        MovementInput();
        GroundDetection();
        CheckStates();
    }
    public void LateUpdate()
    {
        UpdateMouseLook();

        if (Finish) return;
        if (m_CharacterController == null || !m_CharacterController.enabled) return;
        moveDirection = Vector3.Lerp(moveDirection, targetDirection, acceleration * Time.deltaTime);
        m_CollisionFlags = m_CharacterController.Move(moveDirection * Time.deltaTime);
    }
    public void FixedUpdate()
    {
        if (Finish || m_CharacterController == null || !m_CharacterController.enabled || MoveToStarted)
            return;

        GetInput(out float s);
        desiredSpeed = Mathf.Lerp(desiredSpeed, s, Time.fixedDeltaTime * 8);
        speed = desiredSpeed;

        if (isClimbing && m_Ladder != null)
        {
            OnClimbing();
        }
        else
        {
            Move();
        }
    }
    #endregion
    private void OnStateChanged(PlayerState from, PlayerState to)
    {
        if (from == PlayerState.Crouching || to == PlayerState.Crouching)
        {
            DoCrouchTransition();
        }
        else if (from == PlayerState.Sliding || to == PlayerState.Sliding)
        {
            DoCrouchTransition();
        }
        bl_EventHandler.DispatchLocalPlayerStateChange(from, to);
    }
    void MovementInput()
    {
        jumpPressed |= bl_GameInput.Jump();
        if (State is PlayerState.Sliding)
        {
            slideForce -= Time.deltaTime * slideFriction;
            speed = slideForce;
            if (bl_GameInput.Jump())
            {
                State = PlayerState.Jumping;
                m_Jump = true;
            }
            else return;
        }

        if (!m_Jump && State is not PlayerState.Crouching && (Time.time - lastJumpTime) > JumpMinRate)
        {
            m_Jump = bl_GameInput.Jump();
        }

        if (State is not PlayerState.Jumping && State is not PlayerState.Climbing)
        {
            if (forcedCrouch) return;
            if (KeepToCrouch)
            {
                Crounching = bl_GameInput.Crouch();
                if (Crounching != lastCrouchState)
                {
                    OnCrouchChanged();
                    lastCrouchState = Crounching;
                }
            }
            else
            {
                if (bl_GameInput.Crouch(GameInputType.Down))
                {
                    Crounching = !Crounching;
                    OnCrouchChanged();
                }
            }
        }
    }
    void GroundDetection()
    {
        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            OnLand();
        }
        else if (m_PreviouslyGrounded && !m_CharacterController.isGrounded)
        {
            if (!isFalling)
            {
                PostGroundVerticalPos = m_Transform.position.y;
                isFalling = true;
                fallingTime = Time.time;
            }
        }

        if (isFalling)
        {
            VerticalDirectionCheck();
        }

        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            targetDirection.y = 0f;
        }

        if (forcedCrouch)
        {
            if ((Time.frameCount % 10) == 0)
            {
                if (!IsHeadHampered())
                {
                    forcedCrouch = false;
                    State = PlayerState.Idle;
                }
            }
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }
    void Move()
    {
        if (m_CharacterController.isGrounded)
        {
            OnSurface();
            moveDirection.y = -m_StickToGroundForce;
            hasTouchGround = true;
            if (m_Jump || hasPlatformJump)
            {
                DoJump();
            }
        }
        else
        {
            if (State is PlayerState.Dropping)
            {
                OnDropping();
                return;
            }
            else if (State is PlayerState.Gliding)
            {
                OnGliding();
                return;
            }

            OnAir();
        }
    }
    void OnSurface()
    {
        desiredMove.Set(m_Input.x, 0.0f, m_Input.y);
        desiredMove = m_Transform.TransformDirection(desiredMove);

        Physics.SphereCastNonAlloc(m_Transform.localPosition, capsuleRadious, Vector3.down, SurfaceRay, m_CharacterController.height * 0.5f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        desiredMove = Vector3.ProjectOnPlane(desiredMove, SurfaceRay[0].normal);
        targetDirection.x = desiredMove.x * speed;
        targetDirection.z = desiredMove.z * speed;

        SlopeControl();
    }
    void OnAir()
    {
        desiredMove = (m_Transform.forward * Mathf.Clamp01(m_Input.y)) + (m_Transform.right * m_Input.x);
        desiredMove += momentum;

        targetDirection += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        targetDirection.x = (desiredMove.x * speed) * AirControlMultiplier;
        targetDirection.z = (desiredMove.z * speed) * AirControlMultiplier;

        momentum = Vector3.Lerp(momentum, Vector3.zero, Time.fixedDeltaTime * momentunDecaySpeed);
    }
    void OnLand()
    {
        if (isClimbing) return;

        if (!overrideNextLandEvent)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());

            isFalling = false;
            float fallDistance = CalculateFall();
            bl_EventHandler.DispatchPlayerLandEvent(fallDistance);
        }
        else overrideNextLandEvent = false;

        haslanding = true;
        JumpDirection = 0;
        targetDirection.y = 0f;
        m_Jumping = false;
        m_CharacterController.stepOffset = defaultStepOffset;
        if (State is not PlayerState.Crouching)
            State = PlayerState.Idle;

    }
    void OnCrouchChanged()
    {
        if (Crounching)
        {
            State = PlayerState.Crouching;

            if (canSlide && VelocityMagnitude > WalkSpeed && GetLocalVelocity().z > 0.1f)
            {
                DoSlide();
            }
        }
        else
        {
            if (!IsHeadHampered())
            {
                State = PlayerState.Idle;
            }
            else forcedCrouch = true;
        }
    }
    public void DoCrouchTransition()
    {
        StopCoroutine(nameof(CrouchTransition));
        StartCoroutine(nameof(CrouchTransition));
    }
    IEnumerator CrouchTransition()
    {
        bool isCrouch = Crounching || State == PlayerState.Sliding;
        float height = isCrouch ? crouchHeight : defaultHeight;
        Vector3 center = new Vector3(0, height * 0.5f, 0);
        Vector3 cameraPosition = CameraRoot.localPosition;
        Vector3 verticalCameraPos = isCrouch ? new Vector3(cameraPosition.x, defaultCameraRPosition.y - (defaultHeight - height), cameraPosition.z) : defaultCameraRPosition;

        float originHeight = m_CharacterController.height;
        Vector3 originCenter = m_CharacterController.center;
        Vector3 originCameraPosition = cameraPosition;

        float d = 0;
        while (d < 1)
        {
            d += Time.deltaTime / crouchTransitionSpeed;
            m_CharacterController.height = Mathf.Lerp(originHeight, height, d);
            m_CharacterController.center = Vector3.Lerp(originCenter, center, d);
            CameraRoot.localPosition = Vector3.Lerp(originCameraPosition, verticalCameraPos, d);
            yield return null;
        }
    }
    public override void DoJump()
    {
        momentum = desiredMove * jumpMomentumBooster;
        moveDirection.y = (hasPlatformJump) ? PlatformJumpForce : jumpSpeed;
        targetDirection.y = moveDirection.y;
        PlayJumpSound();
        m_Jump = false;
        m_Jumping = true;
        hasPlatformJump = false;
        if (State == PlayerState.Sliding) mouseLook.SetTiltAngle(0);
        Crounching = false;
        State = PlayerState.Jumping;
        lastJumpTime = Time.time;
        m_CharacterController.stepOffset = 0;
        m_CollisionFlags = m_CharacterController.Move(moveDirection * Time.deltaTime);
    }
    public override void DoSlide()
    {
        if ((Time.time - lastSlideTime) < slideTime * slideCoolDown) return;
        if (m_Jumping) return;

        Vector3 startPosition = (m_Transform.position - feetPositionOffset) + (m_Transform.forward * m_CharacterController.radius);
        if (Physics.Linecast(startPosition, startPosition + m_Transform.forward)) return;

        State = PlayerState.Sliding;
        slideForce = slideSpeed;
        speed = slideSpeed;
        mouseLook.SetTiltAngle(slideCameraTiltAngle);
        if (slideSound != null)
        {
            m_AudioSource.clip = slideSound;
            m_AudioSource.volume = 0.7f;
            m_AudioSource.Play();
        }
        mouseLook.UseOnlyCameraRotation();
        this.InvokeAfter(slideTime, () =>
        {
            if (Crounching && !bl_UtilityHelper.isMobile)
                State = PlayerState.Crouching;
            else if (State != PlayerState.Jumping)
                State = PlayerState.Idle;

            Crounching = false;
            DoCrouchTransition();
            lastSlideTime = Time.time;
            mouseLook.SetTiltAngle(0);
            mouseLook.PortBodyOrientationToCamera();
        });
    }
    void SlopeControl()
    {
        float angle = Vector3.Angle(Vector3.up, surfaceNormal);

        if (angle <= m_CharacterController.slopeLimit || angle >= 75) return;

        Vector3 direction = Vector3.up - surfaceNormal * Vector3.Dot(Vector3.up, surfaceNormal);
        float speed = slideSpeed + 1 + Time.deltaTime;

        targetDirection += direction * -speed;
        targetDirection.y = targetDirection.y - surfacePoint.y;
    }
    public override void DoDrop()
    {
        if (isGrounded)
        {
            Debug.Log("Can't drop when player is in a surface");
            return;
        }
        State = PlayerState.Dropping;
    }
    void OnDropping()
    {
        float tilt = Mathf.InverseLerp(0, 90, mouseLook.VerticalAngle);
        tilt = Mathf.Clamp01(tilt);
        if (mouseLook.VerticalAngle <= 0 || mouseLook.VerticalAngle >= 180) tilt = 0;
        desiredMove = headRoot.forward * Mathf.Clamp01(m_Input.y);
        if (desiredMove.y > 0) desiredMove.y = 0;

        float dropSpeed = Mathf.Lerp(m_GravityMultiplier * dropTiltSpeedRange.x, m_GravityMultiplier * dropTiltSpeedRange.y, tilt);
        targetDirection = Physics.gravity * dropSpeed * Time.fixedDeltaTime;
        targetDirection += desiredMove * dropControlSpeed;

        m_CollisionFlags = m_CharacterController.Move(targetDirection * Time.fixedDeltaTime);
    }
    public override void DoGliding()
    {
        if (isGrounded)
        {
            Debug.Log("Can't gliding when player is in a surface");
            return;
        }
        State = PlayerState.Gliding;
    }
    void OnGliding()
    {
        desiredMove = (m_Transform.forward * m_Input.y) + (m_Transform.right * m_Input.x);
        float airControlMult = AirControlMultiplier * 5;
        float gravity = m_GravityMultiplier * 15;

        targetDirection = Physics.gravity * gravity * Time.fixedDeltaTime;
        targetDirection.x = (desiredMove.x * speed) * airControlMult;
        targetDirection.z = (desiredMove.z * speed) * airControlMult;

        m_CollisionFlags = m_CharacterController.Move(targetDirection * Time.fixedDeltaTime);
    }
    void OnClimbing()
    {
        if (m_Ladder.Status == bl_Ladder.LadderStatus.Detaching)
        {
            if (!m_Ladder.Exiting)
            {
                m_Ladder.Exiting = true;
                bool wasControllable = isControlable;
                isControlable = false;

                StartCoroutine(MoveTo(m_Ladder.GetNearestExitPosition(m_Transform), () =>
                {
                    SetActiveClimbing(false);
                    m_Ladder.JumpOut();
                    m_Ladder = null;
                    isControlable = true;
                    bl_EventHandler.onPlayerLand(0.5f);
                }));
            }
        }
        else
        {
            desiredMove = m_Ladder.transform.rotation * forwardVector * m_Input.y;
            targetDirection.y = desiredMove.y * climbSpeed;
            targetDirection.x = desiredMove.x * climbSpeed;
            targetDirection.z = desiredMove.z * climbSpeed;

            if (jumpPressed)
            {
                SetActiveClimbing(false);
                m_Ladder.JumpOut();
                m_Ladder = null;

                targetDirection = -m_Transform.forward * 20;
                targetDirection.y = jumpSpeed;
                lastJumpTime = Time.time;
                jumpPressed = false;
                m_Jump = false;
            }
            m_CollisionFlags = m_CharacterController.Move(targetDirection * Time.smoothDeltaTime);

            if (m_Ladder != null) m_Ladder.WatchLimits();
        }
    }
    private float CalculateFall()
    {
        float fallDistance = HigherPointOnJump - m_Transform.position.y;
        if (JumpDirection == -1)
        {
            float normalized = PostGroundVerticalPos - m_Transform.position.y;
            fallDistance = Mathf.Abs(normalized);
        }

        if (FallDamage && hasTouchGround && haslanding)
        {
            if (JumpInmune)
            {
                JumpInmune = false;
                return fallDistance;
            }
            if ((Time.time - fallingTime) <= 0.4f)
            {
                bl_EventHandler.DispatchPlayerLandEvent(0.2f);
                return fallDistance;
            }

            float ave = fallDistance / DeathFallDistance;
            if (fallDistance > SafeFallDistance)
            {
                int damage = Mathf.FloorToInt(ave * 100);
                playerReferences.playerHealthManager.DoFallDamage(damage);
            }

            PlayLandingSound(ave);
            fallingTime = Time.time;
        }
        else PlayLandingSound(1);
        return fallDistance;
    }
    void VerticalDirectionCheck()
    {
        if (m_Transform.position.y == PostGroundVerticalPos) return;

        if (JumpDirection == 0)
        {
            JumpDirection = (m_Transform.position.y > PostGroundVerticalPos) ? 1 : -1;
        }
        else if (JumpDirection == 1)
        {
            if (m_Transform.position.y < PostGroundVerticalPos)
            {
                HigherPointOnJump = PostGroundVerticalPos;
            }
            else
            {
                PostGroundVerticalPos = m_Transform.position.y;
            }
        }
        else
        {

        }
    }
    private void GetInput(out float outputSpeed)
    {
        if (!isControlable)
        {
            m_Input = Vector2.zero;
            outputSpeed = 0;
            return;
        }

        HorizontalInput = bl_GameInput.Horizontal;
        VerticalInput = bl_GameInput.Vertical;

        if (State == PlayerState.Sliding)
        {
            VerticalInput = 1;
            HorizontalInput = 0;
        }

        m_Input.Set(HorizontalInput, VerticalInput);

        float inputMagnitude = m_Input.magnitude;
        if (State is PlayerState.Dropping || State is PlayerState.Gliding) { outputSpeed = 0; return; }

        if (State is not PlayerState.Climbing && State is not PlayerState.Sliding)
        {
            if (inputMagnitude > 0 && State is not PlayerState.Crouching)
            {
                if (VelocityMagnitude > 0)
                {
                    float forwardVelocity = GetLocalVelocity().z;

                    if (bl_GameInput.Run() && forwardVelocity > 0.1f)
                    {
                        if (runToAimBehave is PlayerRunToAimBehave.AimWhileRunning)
                            State = PlayerState.Running;
                        else if (runToAimBehave is PlayerRunToAimBehave.StopRunning)
                            State = isAiming ? PlayerState.Walking : PlayerState.Running;
                    }
                    else if (canStealthMode && bl_GameInput.Stealth() && VelocityMagnitude > 0.1f)
                    {
                        State = PlayerState.Stealth;
                    }
                    else
                    {
                        State = PlayerState.Walking;
                    }

                }
                else
                {
                    if (State is not PlayerState.Jumping)
                    {
                        State = PlayerState.Idle;
                    }
                }

            }
            else if (m_CharacterController.isGrounded)
            {
                if (State is not PlayerState.Jumping && State is not PlayerState.Crouching)
                {
                    State = PlayerState.Idle;
                }
            }
        }

        if (Crounching)
        {
            outputSpeed = (State == PlayerState.Crouching) ? crouchSpeed : runSpeed;
            if (State == PlayerState.Sliding)
            {
                outputSpeed += 1;
            }
        }
        else
        {
            outputSpeed = (State is PlayerState.Running && m_CharacterController.isGrounded) ? runSpeed : WalkSpeed;
            if (State == PlayerState.Stealth)
            {
                outputSpeed = stealthSpeed;
            }
        }
        if (inputMagnitude > 1)
        {
            m_Input.Normalize();
        }

        if (RunFovEffect)
        {
            float rf = (State == PlayerState.Running && m_CharacterController.isGrounded) ? runFOVAmount : 0;
            RunFov = Mathf.Lerp(RunFov, rf, Time.deltaTime * 6);
        }
    }
    public override void Stop()
    {
        m_Input = Vector2.zero;
        moveDirection = Vector3.zero;
        desiredMove = Vector3.zero;
        targetDirection = Vector3.zero;
        State = PlayerState.Idle;
    }
    public override void PlayFootStepSound()
    {
        if (State == PlayerState.Sliding) return;
        if (!m_CharacterController.isGrounded && !isClimbing)
            return;

        if (!isClimbing)
        {
            if (State == PlayerState.Stealth || State == PlayerState.Crouching)
            {
                footstep?.SetVolumeMuliplier(footstep.stealthModeVolumeMultiplier);
            }
            else footstep?.SetVolumeMuliplier(1f);

            footstep?.DetectAndPlaySurface();
        }
        else
        {
            footstep?.PlayStepForTag("Generic");
        }
    }
    public override void PlatformJump(float force)
    {
        hasPlatformJump = true;
        PlatformJumpForce = force;
        JumpInmune = true;
    }
    public override void UpdateMouseLook()
    {
        mouseLook.Update();

        if (!isClimbing)
        {
            mouseLook.UpdateLook(m_Transform, headRoot);
        }
        else
        {
            mouseLook.UpdateLook(m_Transform, headRoot, m_Ladder);
        }

    }
    private void CheckStates()
    {
        if (lastState == State) return;
        OnStateChanged(lastState, State);
        lastState = State;
    }
    private void PlayLandingSound(float vol = 1)
    {
        vol = Mathf.Clamp(vol, 0.05f, 1);
        m_AudioSource.clip = landSound;
        m_AudioSource.volume = vol;
        m_AudioSource.Play();
    }
    private void PlayJumpSound()
    {
        m_AudioSource.volume = 1;
        m_AudioSource.clip = jumpSound;
        m_AudioSource.Play();
    }
    public bool IsHeadHampered()
    {
        Vector3 origin = m_Transform.localPosition + m_CharacterController.center + Vector3.up * m_CharacterController.height * 0.5F;
        float dist = 2.05f - m_CharacterController.height;
        return Physics.Raycast(origin, Vector3.up, dist);
    }
    #region External Events
    void OnRoundEnd()
    {
        Finish = true;
        isControlable = false;
        Stop();
    }
    void OnChangeWeapon(int id)
    {
        isAiming = false;
        var currentWeapon = playerReferences.gunManager.GetCurrentWeapon();
        if (currentWeapon != null)
        {
            WeaponWeight = currentWeapon.WeaponWeight;
        }
        else
        {
            WeaponWeight = 0;
        }
    }
    void OnMatchStart()
    {
        isControlable = true;
    }
    void OnGameSettingsChanged() => mouseLook.FetchSettings();
    void OnAimChange(bool aim)
    {
        isAiming = aim;
        mouseLook.OnAimChange(aim);
    }
    #endregion
    private void SetActiveClimbing(bool active)
    {
        isClimbing = active;
        State = (isClimbing) ? PlayerState.Climbing : PlayerState.Idle;
        if (isClimbing) bl_InputInteractionIndicator.ShowIndication(bl_Input.GetButtonName("Jump"), bl_GameTexts.JumpOffLadder);
        else bl_InputInteractionIndicator.SetActiveIfSame(false, bl_GameTexts.JumpOffLadder);
    }
    IEnumerator MoveTo(Vector3 position, Action onFinish)
    {
        if (m_Transform == null) m_Transform = transform;

        float t = 0;
        Vector3 from = m_Transform.localPosition;
        m_CharacterController.enabled = false;
        while (t < 1)
        {
            t += Time.deltaTime / 0.5f;
            m_Transform.localPosition = Vector3.Lerp(from, position, t);
            yield return null;
        }
        m_CharacterController.enabled = true;
        onFinish?.Invoke();
    }
    /// <param name="other"></param>
    private void CheckLadderTrigger(Collider other)
    {
        // if the collider doesn't have a parent, it's not a valid ladder
        if (isClimbing || other.transform.parent == null)
            return;

        var ladder = other.GetComponentInParent<bl_Ladder>();
        if (ladder == null || !ladder.CanUse)
            return;

        // Enter in a ladder trigger

        Stop();
        m_Ladder = ladder;
        // setup the in and out position based on the player height
        ladder.SetUpBounds(playerReferences);

        bool wasControllable = isControlable;
        isControlable = false;
        JumpInmune = true;
        jumpPressed = false;

        SetActiveClimbing(true);

        // get the position to automatically translate the player to start climbing/down-climbing
        Vector3 startPos = ladder.GetAttachPosition(other, m_CharacterController.height);
        overrideNextLandEvent = true;
        // move the player to the start position
        StartCoroutine(MoveTo(startPos, () =>
        {
            // after finish the position adjustment
            isControlable = wasControllable;
            ladder.Status = bl_Ladder.LadderStatus.Climbing;

            // now the movement will be handled in OnClimbing() function
        }));
    }
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        CheckLadderTrigger(other);
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        surfaceNormal = hit.normal;
        surfacePoint = hit.point;
        Rigidbody body = hit.collider.attachedRigidbody;
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
    internal float _speed = 0;
    public float speed
    {
        get
        {
            return _speed;
        }
        set
        {
            _speed = value - WeaponWeight;
            float min = 1.75f;
            if (State == PlayerState.Stealth)
            {
                min = 1;
            }
            _speed = Mathf.Max(_speed, min);
        }
    }
    public override float GetCurrentSpeed()
    {
        return speed;
    }
    public override float GetSpeedOnState(PlayerState playerState, bool includeModifiers)
    {
        switch (playerState)
        {
            case PlayerState.Walking:
                return includeModifiers ? WalkSpeed - WeaponWeight : WalkSpeed;
            case PlayerState.Running:
                return includeModifiers ? runSpeed - WeaponWeight : runSpeed;
            case PlayerState.Crouching:
                return includeModifiers ? crouchSpeed - WeaponWeight : crouchSpeed;
            case PlayerState.Dropping:
                return dropTiltSpeedRange.y;
        }
        return includeModifiers ? WalkSpeed - WeaponWeight : WalkSpeed;
    }
    public override PlayerRunToAimBehave GetRunToAimBehave()
    {
        return runToAimBehave;
    }
    public override MouseLookBase GetMouseLook()
    {
        return mouseLook;
    }
    public override bl_Footstep GetFootStep()
    {
        return footstep;
    }
    public override float GetSprintFov()
    {
        return RunFov;
    }
    /// <param name="vertical"></param>
    public override float GetHeadBobMagnitudes(bool vertical)
    {
        if (vertical) return headVerticalBobMagnitude;
        return headBobMagnitude;
    }
    public Vector3 GetLocalVelocity() => m_Transform.InverseTransformDirection(Velocity);
    public Vector3 MovementDirection => targetDirection;
    public override bool isGrounded { get { return m_CharacterController.isGrounded; } }

    [SerializeField]
    public class LerpControlledBob
    {
        public float BobDuration;
        public float BobAmount;
        private float m_Offset = 0f;

        public float Offset()
        {
            return m_Offset;
        }
        public IEnumerator DoBobCycle()
        {
            // make the camera move down slightly
            float t = 0f;
            while (t < BobDuration)
            {
                m_Offset = Mathf.Lerp(0f, BobAmount, t / BobDuration);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            // make it move back to neutral
            t = 0f;
            while (t < BobDuration)
            {
                m_Offset = Mathf.Lerp(BobAmount, 0f, t / BobDuration);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            m_Offset = 0f;
        }
    }
}