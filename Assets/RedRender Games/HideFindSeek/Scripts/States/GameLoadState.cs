using System.IO;
using System.Linq;
using Game.Config;
using Injection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Game.States
{
    public sealed class GameLoadState : GameState
    {
        [Inject] private GameConfig _config;
        [Inject] private Context _context;
        [Inject] private GameStateManager _gameStateManager;

        public override void Initialize()
        {
            var model = GameModel.Load(_config);
            _context.Install(model);

            var path = SceneUtility.GetScenePathByBuildIndex(model.Level);

            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
            SceneManager.LoadScene(Path.GetFileNameWithoutExtension(path), LoadSceneMode.Additive);
        }

        public override void Dispose()
        {
            SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
        }

        private void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            SceneManager.SetActiveScene(scene);

            var vertices = NavMesh.CalculateTriangulation().vertices;
            var bounds = Vector4.zero;

            bounds.x = vertices.Min(temp => temp.x);
            bounds.y = vertices.Min(temp => temp.z);
            bounds.z = vertices.Max(temp => temp.x);
            bounds.w = vertices.Max(temp => temp.z);

            _context.Get<GameModel>().LevelBounds = bounds;

            var level = scene.GetRootGameObjects()[0].GetComponent<LevelView>();

            _context.Install(level);

            // Apply Cyber Glitch Hunt Reskin
            var gameView = _context.Get<GameView>();
            Game.Utilities.RuntimeReskinner.Reskin(level, gameView);

            _gameStateManager.SwitchToState(typeof(GameMenuState));
        }
    }
}