using Injection;
using UnityEngine.SceneManagement;

namespace Game.States
{
    public sealed class GameUnloadState : GameState
    {
        [Inject] private GameStateManager _gameStateManager;

        public override void Initialize()
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));
            SceneManager.sceneUnloaded += SceneManagerOnsceneUnloaded;
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1), UnloadSceneOptions.None);
        }
        
        public override void Dispose()
        {
            SceneManager.sceneUnloaded -= SceneManagerOnsceneUnloaded;
        }

        private void SceneManagerOnsceneUnloaded(Scene scene)
        {
            _gameStateManager.SwitchToState(typeof(GameLoadState));
        }
    }
}