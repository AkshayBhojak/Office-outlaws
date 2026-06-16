using System;
using Game.Core;
using Game.States;
using Injection;
using UnityEngine;

namespace Game
{
    public sealed class GameStartBehaviour : MonoBehaviour
    {
        private Timer _timer;

        public Context Context { get; private set; }

        private void Start()
        {
            _timer = new Timer();

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Application.runInBackground = true;

            // Instantiate AdManager
            if (FindObjectOfType<Game.Ads.AdManager>() == null)
            {
                new GameObject("AdManager", typeof(Game.Ads.AdManager));
            }

            var context = new Context();

            context.Install(
                new GameManager(),
                new GameStateManager(),
                new Injector(context)
            );
            context.Install(GetComponents<Component>());
            context.Install(_timer);
            context.ApplyInstall();

            context.Get<GameStateManager>().SwitchToState(typeof(GameInitializeState));

            Context = context;
        }

        private void Update()
        {
            _timer.Update();
        }

        private void LateUpdate()
        {
            _timer.LateUpdate();
        }
    }
}