using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Battle
{
    public class CharacterAnimator
    {
        private const string STATE_ATTACK = "Attack";
        private const string STATE_IDLE = "Idle";
        private const string STATE_JUMP = "Jump";
        private const string STATE_LAND = "Land";
        private const string STATE_HIT = "Hit";
        private const string STATE_MOVE = "Move";
        private const string STATE_CANCEL = "Cancel";
        private const string STATE_FALL = "Fall";
        private const string STATE_SLIDING = "Sliding";
        private const string STATE_CROUCH = "Crouch";
        private const string STATE_INTERACTION = "Interaction";
        private const string PARAMETERS_SPEED = "Speed";
        private const string PARAMETERS_CROUCH = "Crouch";
        private const string PARAMETERS_INTERACTION = "Interaction";
        private readonly Animator _animator;
        private readonly Character _character;
        private readonly Dictionary<string, int> _stateToHash = new();
        private Dictionary<int, string> _hashToState = new();
        private string preTrigger = STATE_IDLE;
        private Coroutine _awaitAnimCroutine;
        private Coroutine _awaitSlidCoroutine;
        private Coroutine _stopingWalkCoroutine;

        public CharacterAnimator(Character character, Animator animator)
        {
            _animator = animator;
            _character = character;
            CachingAnimatorStateHashes();
            AddEvent();
        }
        private void AddEvent()
        {
            _character.OnIdle += () => SetSpeed(0f);
            _character.OnIdle += () => SetTrigger(STATE_IDLE, 0);
            _character.OnIdle += () => SetDir(Vector3.zero);
            _character.OnMove += (dir) => SetTrigger(STATE_MOVE, 0);
            _character.OnMove += (dir) => SetSpeed(0.5f);
            _character.OnMove += SetDir;
            _character.OnRun += (dir) => SetTrigger(STATE_MOVE, 0);
            _character.OnRun += (dir) => SetSpeed(1f);
            _character.OnRun += SetDir;
            _character.OnAttack += () => DoCalmDown(STATE_ATTACK, 1);
            _character.OnAttack += () => SetTrigger(STATE_ATTACK, 1);
            _character.OnJump += () => SetTrigger(STATE_JUMP, 0);
            _character.OnFall += () => SetTrigger(STATE_FALL, 0);
            _character.OnLand += () => DoCalmDown(STATE_LAND, 0);
            _character.OnLand += () => SetTrigger(STATE_LAND, 0);
            _character.OnHit += () => DoCalmDown(STATE_HIT, 0);
            _character.OnHit += () => SetTrigger(STATE_HIT, 0);
            _character.OnSlide += DoCalmDownSlide;
            _character.OnSlide += () => SetTrigger(STATE_SLIDING, 0);
            _character.OnSlideEnd += () => SetTrigger(STATE_MOVE, 0);
            _character.OnStandup += () => SetBoolean(PARAMETERS_INTERACTION, false);
            _character.OnStandup += () => SetBoolean(PARAMETERS_CROUCH, false);
            _character.OnCancel += () => DoCalmDown(STATE_CANCEL, 1);
            _character.OnCancel += () => SetTrigger(STATE_CANCEL, 0);
            _character.OnInteraction += () => SetBoolean(PARAMETERS_INTERACTION, true);
            _character.OnInteraction += () => SetBoolean(PARAMETERS_CROUCH, true);
        }
        private void CachingAnimatorStateHashes()
        {
            _stateToHash.Add(STATE_IDLE, Animator.StringToHash(STATE_IDLE));
            _stateToHash.Add(STATE_MOVE, Animator.StringToHash(STATE_MOVE));
            _stateToHash.Add(STATE_ATTACK, Animator.StringToHash(STATE_ATTACK));
            _stateToHash.Add(STATE_JUMP, Animator.StringToHash(STATE_JUMP));
            _stateToHash.Add(STATE_LAND, Animator.StringToHash(STATE_LAND));
            _stateToHash.Add(STATE_HIT, Animator.StringToHash(STATE_HIT));
            _stateToHash.Add(STATE_CANCEL, Animator.StringToHash(STATE_CANCEL));
            _stateToHash.Add(STATE_FALL, Animator.StringToHash(STATE_FALL));
            _stateToHash.Add(STATE_SLIDING, Animator.StringToHash(STATE_SLIDING));
            _hashToState = _stateToHash.ToDictionary(kv => kv.Value, kv => kv.Key);
        }
        private bool IsSame(int exitState, int layer)
        {
            var state = _animator.GetCurrentAnimatorStateInfo(layer);
            return state.shortNameHash == exitState;
        }
        private bool IsExit(int exitState, int layer)
        {
            var state = _animator.GetCurrentAnimatorStateInfo(layer);
            var outState = state.shortNameHash != exitState;
            var outRange = 1 <= state.normalizedTime;
            return outState || outRange;
        }
        private void SetSpeed(float speed)
        {
            _animator.SetFloat(PARAMETERS_SPEED, speed);
            if (_stopingWalkCoroutine != null)
            {
                CoroutineRunner.instance.StopCoroutine(_stopingWalkCoroutine);
            }
            _stopingWalkCoroutine = CoroutineRunner.instance.StartCoroutine(StopingWalk());
        }
        private void SetTrigger(string trigger, int layer)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(layer);
            var state = stateInfo.shortNameHash;
            var loop = stateInfo.loop;
            if (!loop && preTrigger == trigger && state == _stateToHash[trigger])
            {
                _animator.Play(state, layer, 0f);
            }
            else
            {
                _animator.ResetTrigger(preTrigger);
                _animator.SetTrigger(trigger);
            }
            preTrigger = trigger;
        }
        private void SetBoolean(string boolean, bool value)
        {
            _animator.SetBool(boolean, value);
        }
        private void DoCalmDown(string exitState, int layer)
        {
            if (_awaitAnimCroutine != null)
            {
                CoroutineRunner.instance.StopCoroutine(_awaitAnimCroutine);
            }
            _awaitAnimCroutine = CoroutineRunner.instance.StartCoroutine(AwaitAnimCalmDown(_stateToHash[exitState], layer));
        }
        private void DoCalmDownSlide()
        {
            if (_awaitSlidCoroutine != null)
            {
                CoroutineRunner.instance.StopCoroutine(_awaitSlidCoroutine);
            }
            _awaitSlidCoroutine = CoroutineRunner.instance.StartCoroutine(AwaitAnimStandUp());
        }
        private IEnumerator StopingWalk()
        {
            var t = 1f;
            var duration = 1f;
            var preSpeed = _animator.GetFloat(PARAMETERS_SPEED);
            while (0 < t)
            {
                yield return new WaitForFixedUpdate();
                t = Mathf.Clamp01(t - Time.fixedDeltaTime / duration);
                _animator.SetFloat(PARAMETERS_SPEED, t * preSpeed);
            }
        }
        private IEnumerator AwaitAnimStandUp()
        {
            yield return new WaitUntil(() => IsSame(_stateToHash[STATE_SLIDING], 0));
            yield return new WaitUntil(() => IsExit(_stateToHash[STATE_SLIDING], 0));
            _character.DoStandUp();
        }
        private IEnumerator AwaitAnimCalmDown(int exitState, int layer)
        {
            yield return new WaitUntil(() => IsSame(exitState, layer));
            yield return new WaitUntil(() => IsExit(exitState, layer));
            _character.CalmDown();
        }
        private Coroutine _durationDirCoroutin;
        private Vector3 _preDir;
        private void SetDir(Vector3 dir)
        {
            if (_preDir == dir)
            {
                return;
            }
            if (_durationDirCoroutin != null)
            {
                CoroutineRunner.instance.StopCoroutine(_durationDirCoroutin);
            }
            dir.z = Mathf.Clamp01(dir.z);
            _durationDirCoroutin = CoroutineRunner.instance.StartCoroutine(DurationDir(dir));
            _preDir = dir;
        }
        private IEnumerator DurationDir(Vector3 dir)
        {
            var pre = new Vector3(_animator.GetFloat("Horizontal"), 0, _animator.GetFloat("Vertical"));
            var duration = 0.5f;
            var t = 0f;
            while (t < 1)
            {
                t = Mathf.Clamp01(t + Time.deltaTime / duration);
                _animator.SetFloat("Horizontal", Vector3.Lerp(pre, dir, t).x);
                _animator.SetFloat("Vertical", Vector3.Lerp(pre, dir, t).z);
                yield return null;
            }
        }
    }
}
