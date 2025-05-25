using System;
using UnityEngine;

namespace Battle
{
    public enum Team : byte { None, Monster, NPC, Player }
    public class Henchmen : IController
    {
        CharacterComponent character;
        public static event Action<Henchmen> OnSpawnEvent;
        public static event Action<Henchmen> OnDestroyEvent;
        public Team Team;
        public bool HasDependency { get; set; }
        public Vector3 Position => character.transform.position;

        public Henchmen(CharacterComponent character)
        {
            this.character = character;
        }
        public void LookForJob()
        {
            OnSpawnEvent?.Invoke(this);
        }
        public void LeaveJob()
        {
            OnDestroyEvent?.Invoke(this);
        }
        Vector3 prePosition;
        Vector3 targetPosition;
        public void MoveTo(Vector3 position)
        {
            targetPosition = position;
        }
        public void Update()
        {
            PlayerControl.UpdateLand(character);
            if (prePosition != targetPosition)
            {
                prePosition = targetPosition;
                character.DoMove(targetPosition);
            }
        }
    }
}
