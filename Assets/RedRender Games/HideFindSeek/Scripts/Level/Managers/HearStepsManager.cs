using System.Collections.Generic;
using Game.Core;
using Game.Core.UI;
using Injection;
using UnityEngine;
using Utils;

namespace Game
{
    public sealed class HearStepsManager : Mediator<LevelView>
    {
        [Inject] private Timer _timer;
        [Inject] private GameView _gameView;
        [Inject] private GameManager _gameManager;
        [Inject] private LevelView _levelView;

        private readonly bool _isShowEffects;
        private readonly List<ParticleSystem> _effects;
        private AnimationType[] _animations;

        public HearStepsManager(bool isShowEffects)
        {
            _isShowEffects = isShowEffects;
            _effects = new List<ParticleSystem>();
        }

        protected override void Show()
        {
            _animations = new AnimationType[_view.Units.Length];

            foreach (var door in _levelView.Doors)
            {
                door.COLLISION += OnCollisionDetected;
            }

            _timer.POST_TICK += TimerOnPostTick;
        }
        
        protected override void Hide()
        {
            foreach (var door in _levelView.Doors)
            {
                door.COLLISION -= OnCollisionDetected;
            }

            _gameView.StepsEffectPool.ReleaseAllInstances();
            _effects.Clear();

            _timer.POST_TICK -= TimerOnPostTick;
        }

        private void TimerOnPostTick()
        {
            if (null == _gameManager.Player)
                return;

            for (int i = 0; i < _view.Units.Length; i++)
            {
                var unit = _view.Units[i];
                
                if (unit.AnimationType == AnimationType.Walk && _animations[i] != AnimationType.Walk)
                {
                    if (_isShowEffects && _gameManager.Player.View != unit)
                    {
                        CreateEffect(unit);
                    }

                    if (unit.IsVictim)
                    {
                        _gameManager.FireSound(unit.transform.position);
                    }
                }

                _animations[i] = unit.AnimationType;
            }

            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                if (!_effects[i].IsAlive())
                {
                    _gameView.StepsEffectPool.Release(_effects[i]);
                    _effects.RemoveAt(i);
                }
            }
        }

        private void CreateEffect(UnitView unit)
        {
            var effect = _gameView.StepsEffectPool.Get<ParticleSystem>();
            effect.transform.position = unit.transform.position;
            effect.transform.localEulerAngles = new Vector3(90, unit.Rotation, 0);

            _effects.Add(effect);
        }

        private void OnCollisionDetected(Collider collider)
        {
            if (collider.gameObject.layer == LayerUtils.GetVictimLayer(true))
            {
                _gameManager.FireSound(collider.transform.position);
            }
        }
    }
}