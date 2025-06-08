using UnityEngine;

namespace TopDown
{
    public class GameManagerComponent : MonoBehaviour
    {
        void Awake()
        {
            var play = GmaeManager.Start;
        }
    }

    public static class GmaeManager
    {
        public static bool Start { get; }
        static ModeSet modeSet;
        static IGameMode gameMode;
        static MapType PlayMode = MapType.Lobby;

        static bool IsNull(object item, string name)
        {
            if (item == null)
            {
                Debug.Log($"{DM_ERROR.NOT_FOUND} {name}");
                ApplicationControl.QuitGame();
            }
            return item == null;
        }
        static GmaeManager()
        {
            InitializeModeSet();
            SetGameMode();
        }
        static void InitializeModeSet()
        {
            modeSet = Resources.Load<ModeSet>(nameof(ModeSet));
            if (IsNull(modeSet, nameof(modeSet))) return;
            modeSet.MapType = PlayMode;
        }
        static void SetGameMode()
        {
            if (IsNull(modeSet, nameof(modeSet))) return;
            gameMode = GameModeFactory.CreateMode(modeSet.MapType);

            if (IsNull(gameMode, nameof(gameMode))) return;
            gameMode.OnQuit += OnExitGameMode;
            gameMode.Load(modeSet);
        }
        static void ClearGameMode()
        {
            gameMode.OnQuit -= OnExitGameMode;
            gameMode = null;
        }
        static void UnloadResource()
        {
        }
        static void OnExitGameMode()
        {
            UnloadResource();
            ClearGameMode();
            SetGameMode();
        }
    }
}
