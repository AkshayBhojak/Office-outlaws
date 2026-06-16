using System.Collections.Generic;
using Game.Config;
using Game.Core;
using Game.Core.UI;
using Injection;
using UnityEngine;

namespace Game
{
    public sealed class CoinsManager : Mediator<LevelView>
    {
        [Inject] private Timer _timer;
        [Inject] private GameConfig _config;
        [Inject] private GameManager _gameManager;
        [Inject] private GameView _gameView;

        private readonly List<ParticleSystem> _effects;

        public CoinsManager()
        {
            _effects = new List<ParticleSystem>();
        }

        protected override void Show()
        {
            _timer.POST_TICK += TimerOnPOST_TICK;
        }

        protected override void Hide()
        {
            _effects.Clear();
            _gameView.SparksEffectPool.ReleaseAllInstances();
            _timer.POST_TICK -= TimerOnPOST_TICK;
        }

        private void TimerOnPOST_TICK()
        {
            var radius = _config.GetValue(GameParam.CoinRadius);

            foreach (var coin in _view.Coins)
            {
                if (!coin.enabled)
                    continue;

                var colliders = Physics.OverlapSphere(coin.transform.position, radius);

                foreach (var collider in colliders)
                {
                    var unit = collider.GetComponent<UnitView>();

                    if (null != unit)
                    {
                        coin.Collect();
                        var effect = _gameView.SparksEffectPool.Get<ParticleSystem>();
                        effect.transform.position = coin.transform.position;

                        effect.Stop(true);
                        effect.Play(true);

                        _effects.Add(effect);
                        _gameManager.CollectCoin(unit, coin.Value);

                        if (unit.IsVictim)
                        {
                            _gameManager.FireSound(coin.transform.position);
                        }
                    }
                }
            }

            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                if (!_effects[i].IsAlive())
                {
                    _gameView.SparksEffectPool.Release(_effects[i]);
                    _effects.RemoveAt(i);
                }
            }
        }
    }
}