using System.Collections;
using UnityEngine;

namespace Battle
{
    public class MonsterJoycon : IController
    {
        public MonsterJoycon(Character character, CharacterController controller)
        {
        }

        public void UpdateInput()
        {
        }
    }

    public class MonsterComponent : CharacterComponent
    {
        [ContextMenu("H")]
        public void Hit()
        {
            character.DoHit();
        }
        protected override void GetAnimatorFromFrame(out CharacterAnimator animator)
        {
            animator = new ZombieAnimator(this.character, this.animator);
        }
        protected override void GetJoyconFromFrame(out IController newJoycon)
        {
            newJoycon = new MonsterJoycon(character, characterController);
        }
        protected override void GetMovementFromFrame(out CharacterMovement newMovement)
        {
            newMovement = new CharacterMovement(character, characterController);
        }
        protected override void OnAwake()
        {
        }
        protected override void OnFixedUpdate()
        {
        }
        protected override void OnHit(HitBoxCollision collision)
        {
            StopAllCoroutines();
            StartCoroutine(KnockbackRoutine(collision.Attacker.Actor.forward));
        }
        protected override void OnLateUpdate()
        {
        }
        protected override void OnUpdate()
        {
        }
        private IEnumerator KnockbackRoutine(Vector3 direction)
        {
            var knockbackDuration = 1.0f;
            var knockbackStrength = 10f;
            float timer = 0f;
            float volume = Mathf.PI * Mathf.Pow(characterController.radius, 2) * characterController.height * 2f;
            Vector3 velocity = direction.normalized * knockbackStrength / volume;
            while (timer < knockbackDuration)
            {
                characterController.Move(velocity * Time.deltaTime);

                timer += Time.deltaTime;
                yield return null;
            }
        }
    }
}
