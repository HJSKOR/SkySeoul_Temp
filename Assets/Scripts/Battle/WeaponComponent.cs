using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Util;

namespace Battle
{
    public class WeaponComponent : MonoBehaviour
    {
        public float maxDistance;
        private Weapon weapon;
        private Bullet bullet;
        private AttackData attackData;
        [Header("Animation")]
        [SerializeField] private UnityEvent onFire;
        [Header("Effect")]
        [SerializeField] ParticleSystem attackEffect;

        public void SetOwner(Character character, Transform actor)
        {
            weapon = new Weapon(character);
            weapon.OnFire += DoAttack;
            bullet = new Bullet(gun: transform, actor);
            onFire.AddListener(bullet.OnFire);
            bullet.OnHit += OnHit;
        }
        private void DoAttack()
        {
            StartCoroutine(DelayAction(onFire.Invoke, attackData.PreDelay));
        }

        private void OnHit(HitBoxCollision collision)
        {
            if (attackEffect == null) return;
            var particle = ParticlePlayer.GetPool(attackEffect).Get();
            particle.transform.position = collision.HitPoint;
            particle.Play();
            StartCoroutine(DelayAction(() => ParticlePlayer.GetPool(attackEffect).Release(particle), attackEffect.totalTime));

        }
        private IEnumerator DelayAction(UnityAction action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        private void Update()
        {
            bullet.MaxDistance = maxDistance;
        }
    }
}
