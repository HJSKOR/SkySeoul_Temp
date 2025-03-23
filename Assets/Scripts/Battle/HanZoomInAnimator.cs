using UnityEngine;

namespace Battle
{
    public class HanZoomInAnimator : CharacterAnimator
    {
        public HanZoomInAnimator(Character character, Animator animator) : base(character, animator)
        {
        }
        private void DoCalmDown()
        {
            _character.CalmDown();
        }
        private void DoStandUp()
        {
            _character.DoStandUp();
        }
        protected override void OnAttack()
        {
            SetTrigger(STATE_ATTACK, 1);
            AwaitExitEvent(STATE_ATTACK, 1, DoCalmDown);
        }
        protected override void OnCalmDown()
        {
            SetTrigger(PARAMETERS_CALMDOWN, 0);
        }
        protected override void OnCancel()
        {
            SetTrigger(STATE_CANCEL, 0);
            AwaitExitEvent(STATE_CANCEL, 1, DoCalmDown);
        }
        protected override void OnFall()
        {
            SetTrigger(STATE_FALL, 0);
        }
        protected override void OnHit()
        {
            SetTrigger(STATE_HIT, 1);
            AwaitExitEvent(STATE_HIT, 1, DoCalmDown);
        }
        protected override void OnIdle()
        {
        }
        protected override void OnInteraction()
        {
        }
        protected override void OnJump()
        {
        }
        protected override void OnLand()
        {
            SetTrigger(STATE_LAND, 0);
            AwaitExitEvent(STATE_LAND, 0, DoCalmDown);
        }
        protected override void OnMove(Vector3 dir)
        {
            SetTrigger(STATE_MOVE, 0);
            SetFloat(_stringToHash[PARAMETERS_SPEED], 0.5f);
            SetFloat(_stringToHash[PARAMETERS_SPEED], 0f,1f);
            UpdateDirection(dir);
        }
        protected override void OnRun(Vector3 dir)
        {
            SetTrigger(STATE_MOVE, 0);
            SetFloat(_stringToHash[PARAMETERS_SPEED], 1f);
            SetFloat(_stringToHash[PARAMETERS_SPEED], 0f, 1f);
            UpdateDirection(dir);
        }
        protected override void OnSlide()
        {
        }
        protected override void OnSlideEnd()
        {
        }
        protected override void OnStandUp()
        {
            SetBoolean(PARAMETERS_INTERACTION, false);
            SetBoolean(PARAMETERS_CROUCH, false);
        }
        private Vector3 _preDir;
        private void UpdateDirection(Vector3 dir)
        {
            if (_preDir == dir)
            {
                return;
            }

            SetFloat(_stringToHash[PARAMETERS_HORIZONTAL], dir.x, 0.5f);
            SetFloat(_stringToHash[PARAMETERS_VERTICAL], dir.z, 0.5f);
            _preDir = dir;
        }
        protected override void OnUse()
        {
            _animator.Play(STATE_ATTACK);
        }
    }
}
