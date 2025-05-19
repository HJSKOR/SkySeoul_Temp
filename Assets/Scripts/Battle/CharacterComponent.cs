using TMPro;
using UnityEngine;

namespace Battle
{
    public abstract class CharacterComponent : MonoBehaviour
    {
        protected Character character { get; private set; }
        private IController joycon;
        private CharacterAnimator characterAnimator;
        private CharacterMovement characterMovement;
        [field: SerializeField] protected Animator animator { get; private set; }
        [field: SerializeField] protected CharacterController characterController { get; private set; }
        [Header("Battle")]
        public float ArmLength;
        public float MoveSpeed = 3f;
        public float JumpPower = 5f;
        public float SlidePower = 3f;
        [SerializeField] private WeaponComponent weapon;
        [SerializeField] private HitBoxComponent hitBox;
        [Header("Debug")]
        [SerializeField] private TextMeshProUGUI ui;

        protected abstract void OnUpdate();
        protected abstract void OnLateUpdate();
        protected abstract void OnFixedUpdate();
        protected abstract void OnAwake();
        protected abstract void GetAnimatorFromFrame(out CharacterAnimator animator);
        protected abstract void GetJoyconFromFrame(out IController newJoycon);
        protected abstract void GetMovementFromFrame(out CharacterMovement newMovement);
        protected abstract void OnHit(HitBoxCollision collision);
        private void Awake()
        {
            character = new();
            hitBox.HitBox.OnCollision += (h) => character.DoHit();
            hitBox.HitBox.OnCollision += OnHit;
            weapon?.SetOwner(character, actor: transform);
            OnAwake();
        }
        private void Update()
        {
            GetAnimatorFromFrame(out var currentAnimator);
            ChangeAnimator(currentAnimator);
            GetJoyconFromFrame(out var currentJoycon);
            ChangeJoycon(currentJoycon);
            GetMovementFromFrame(out var currentMovement);
            ChangeMovement(currentMovement);
            OnUpdate();
        }
        private void LateUpdate()
        {
            OnLateUpdate();
        }
        private void FixedUpdate()
        {
            ui?.SetText(character.BodyState.ToString());
            characterMovement?.UpdateGravity();
            UpdateJoycon();
            OnFixedUpdate();
        }
        private void UpdateJoycon()
        {
            joycon?.UpdateInput();
        }
        private void ChangeAnimator(CharacterAnimator newAnimator)
        {
            if (newAnimator == characterAnimator) return;
            characterAnimator?.Unuse();
            characterAnimator = newAnimator;
            characterAnimator.Use();
        }
        private void ChangeJoycon(IController joycon)
        {
            this.joycon = joycon;
        }
        private void ChangeMovement(CharacterMovement movement)
        {
            if (movement == characterMovement) return;
            characterMovement = movement;
        }
#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (characterMovement != null)
            {
                characterMovement.SlidePower = SlidePower;
                characterMovement.JumpPower = JumpPower;
                characterMovement.MoveSpeed = MoveSpeed;
            }
        }
#endif
    }
}
