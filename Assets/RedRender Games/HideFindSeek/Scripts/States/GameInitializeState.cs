using Game.Config;
using Injection;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.States
{
    public sealed class GameInitializeState : GameState
    {
        public static GameConfig LoadConfig()
        {
            var textAsset = Resources.Load<TextAsset>(GameConfig.Name);
            GameConfig config;
            if (null != textAsset)
            {
                string json = textAsset.text;
                config = JsonConvert.DeserializeObject<GameConfig>(json);
            }
            else
            {
                config = new GameConfig();
            }

            return config;
        }

        [Inject]
        private GameStateManager _gameStateManager;
        [Inject]
        private Context _context;
        
        public override void Initialize()
        {
            _context.Install(LoadConfig());
            _gameStateManager.SwitchToState(typeof(GameLoadState));
        }

        public override void Dispose()
        {
        }
    }
}