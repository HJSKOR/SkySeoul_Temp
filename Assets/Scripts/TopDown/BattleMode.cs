using System;
using System.Collections.Generic;
using UnityEngine;

namespace TopDown
{
    internal class BattleMode : IGameMode
    {
        public event Action OnQuit;
        public event Action OnLoaded;
        ILoad loader;

        public void Load(ModeSet set)
        {
            var name = MapType.Lobby.ToString() + "Loader";
            loader = LoaderFactory.Load<ILoad>(name, name);
            var list = new List<Loader>();
            list.Add(new SceneLoader(MapType.BattleMap.ToString()));
            list.Add(Loader<AudioClip, AudioClip>.GetLoader(nameof(AudioClip)));
            list.Add(Loader<GameObject, ParticleSystem>.GetLoader(nameof(ParticleSystem)));
            loader.Initialize(list);
            loader.Load();
        }
    }
}