using System;

namespace Battle
{
    public class BattleController : IController
    {
        readonly BattleHUD battleHUD = new();

        public void AddCharacter(CharacterComponent character)
        {
            character.Body.HitBox.OnCollision += OnHitCharacter;
        }
        void OnHitCharacter(HitBoxCollision collision)
        {
            if (!collision.Victim.Actor.TryGetComponent<CharacterComponent>(out var character)) return;

            Action<CharacterComponent> updateHUD = character switch
            {
                IPlayer => battleHUD.UpdatePlayer,
                _ => battleHUD.UpdateMonster
            };
            updateHUD.Invoke(character);
            character.HP.Value--;
            updateHUD.Invoke(character);

            if (character.HP.Value > 0) character.DoHit(); else { character.DoDie(); character.Body.HitBox.OnCollision -= OnHitCharacter; }
        }
        public void Update()
        {
        }
    }
}
