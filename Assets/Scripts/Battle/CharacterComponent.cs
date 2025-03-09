using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Battle
{
    public class CharacterComponent : MonoBehaviour
    {
        public float MoveSpeed = 3f;
        public float JumpPower = 5f;
        public float SlidPower = 3f;
        private Character _character;
        private CharacterJoycon _joycon;
        private CharacterAnimator _characterAnimator;
        private CharacterMovement _characterMovement;
        private ShootingView _view;
        private HandIK _handIK;
        [SerializeField, Range(0, 180f)] private float _limitUp;
        [SerializeField, Range(-180f, 0)] private float _limitDown;
        [SerializeField, Range(0, 1000)] private float _mouseSensitivity = 500;
        [SerializeField] private WeaponComponent _weapon;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private TextMeshProUGUI _ui;
        [SerializeField] private Animator _animator;
        [SerializeField] private TriggerEventHandler _groundCheck;
        [SerializeField] private Transform _cross;
        [SerializeField] private Transform _cam;
        public float ArmLength;
        private void Awake()
        {
            _character = new();
            _character.OnSlide += CacheLookDir;
            _character.OnInteraction += CacheLookDir;
            _weapon.SetOwner(_character);
            _joycon = new CharacterJoycon(_character, _controller);
            _characterAnimator = new CharacterAnimator(_character, _animator);
            _characterMovement = new CharacterMovement(_character, _controller)
            {
                JumpPower = JumpPower,
                MoveSpeed = MoveSpeed,
                SlidPower = SlidPower
            };
            _view = new(transform)
            {
                LimitUp = _limitUp,
                LimitDown = _limitDown,
                MouseSensitivity = _mouseSensitivity
            };
            _handIK = new(_animator);
            _handIK.SetWeight(1f);
            _handIK.SetTartget(_cross);
            var IK = _animator.AddComponent<IKEventHandler>();
            IK.onAnimatorIK.AddListener(OnAnimatorIK);

            _character.OnMove += OnDir;
            _character.OnRun += OnDir;
        }
        private void Update()
        {
            UpdateJoycon();
            UpdateView();

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _controller.transform.rotation = _animator.transform.rotation;
                _animator.transform.localRotation = Quaternion.identity;
                _cam.transform.localRotation = Quaternion.identity;
            }
        }
        private Coroutine _dirCoroutine;
        private Vector3 _pre;
        private void OnDir(Vector3 dir)
        {
            if (_pre == dir)
            {
                return;
            }
            if (_dirCoroutine != null)
            {
                StopCoroutine(_dirCoroutine);
            }
            _dirCoroutine = StartCoroutine(DurationDir(dir));
            _pre = dir;
        }
        private IEnumerator DurationDir(Vector3 dir)
        {
            var @base = _controller.transform;
            dir = @base.forward * dir.z + @base.right * dir.x;
            dir.y = 0;
            dir = dir.normalized;
            var duration = 0.5f;
            var forward = _animator.transform.forward;
            var t = 0f;
            while (t < 1)
            {
                t = Mathf.Clamp01(t + Time.deltaTime / duration);
                _animator.transform.forward = Vector3.Lerp(forward, dir, t);
                yield return null;
            }
        }
        private void FixedUpdate()
        {
            _characterMovement.UpdateGravity();
            _ui.text = _character.BodyState.ToString();
        }
        private void OnAnimatorIK(int layerIndex)
        {
            if (_animator.GetCurrentAnimatorStateInfo(_animator.GetLayerIndex("Upper Boddy")).shortNameHash == Animator.StringToHash("Attack"))
            {
                _handIK.OnAnimatorIK(layerIndex);
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_characterMovement != null)
            {
                _characterMovement.SlidPower = SlidPower;
                _characterMovement.JumpPower = JumpPower;
                _characterMovement.MoveSpeed = MoveSpeed;
            }
            if (_view != null)
            {
                _view.LimitUp = _limitUp;
                _view.LimitDown = _limitDown;
                _view.MouseSensitivity = _mouseSensitivity;
            }
            if (_handIK != null)
            {
                _handIK.ARM_LENGTH = ArmLength;
            }
        }
#endif
        private void UpdateJoycon()
        {
            _joycon.UpdateInput();
        }
        private Vector3 _lockLookDir;
        private void CacheLookDir()
        {
            _lockLookDir = _animator.transform.eulerAngles;
        }
        private void UpdateView()
        {
            _view.UpdateView();
        }
    }
}
