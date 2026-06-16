using System;
using Game.Config;
using Game.Core;
using Game.Player.States;
using Injection;
using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerController : IDisposable
    {
        [Inject] private GameConfig _config;

        private readonly UnitView _view;
        private readonly StateManager<PlayerState> _playerStateManager;

        public UnitView View => _view;

        public Vector2 Position
        {
            get { return _view.Position; }
        }

        public bool IsDied => _view.IsDied();

        public PlayerController(Context context, UnitView player)
        {
            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _view = player;
            _playerStateManager = new StateManager<PlayerState>();
            injector.Inject(_playerStateManager);

            _view.name = "Player";
            _view.SetOutline(context.Get<GameView>().OutlineMaterials);
        }

        public void Dispose()
        {
            _playerStateManager.Dispose();
        }

        public void Live(bool isVictim)
        {
            _view.IsVictim = isVictim;
            _view.IsAI = false;
            _view.IsVissible = true;

            _view.Idle();
            _playerStateManager.SwitchToState(new PlayerLiveState());
        }

        public void Kill()
        {
            _playerStateManager.Dispose();
            _view.Die();
        }

        public Collider RaycastVictim()
        {
            return _view.GetSeenVictim();
        }
    }
}