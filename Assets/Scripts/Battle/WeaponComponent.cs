using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Battle
{
    public class WeaponComponent : MonoBehaviour
    {
        private Weapon _weapon;
        [SerializeField] private UnityEvent _onFire;
        [SerializeField, Range(0, 1f)] private float _lerp;
        [SerializeField] AnimationClip _attackClip;

        public void SetOwner(Character character)
        {
            _weapon = new Weapon(character);
            _weapon.OnFire += DoAttack;
        }
        private void DoAttack()
        {
            var delay = 0f;
            if (_attackClip == null)
            {
                delay = 0;
            }
            else
            {
                delay = _attackClip.length * _lerp;
            }
            StartCoroutine(DelayAttack(delay));
        }
        private IEnumerator DelayAttack(float delay)
        {
            yield return new WaitForSeconds(delay);
            _onFire.Invoke();
        }
    }

}
