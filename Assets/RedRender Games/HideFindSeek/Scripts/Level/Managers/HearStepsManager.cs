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
        private float[] _lastFootprintTime;
        private GameObject[] _activeTrails;

        public HearStepsManager(bool isShowEffects)
        {
            _isShowEffects = isShowEffects;
            _effects = new List<ParticleSystem>();
        }

        protected override void Show()
        {
            _animations = new AnimationType[_view.Units.Length];
            _lastFootprintTime = new float[_view.Units.Length];
            _activeTrails = new GameObject[_view.Units.Length];

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

            if (_activeTrails != null)
            {
                for (int i = 0; i < _activeTrails.Length; i++)
                {
                    if (_activeTrails[i] != null)
                    {
                        GameObject.Destroy(_activeTrails[i]);
                    }
                }
                _activeTrails = null;
            }

            _timer.POST_TICK -= TimerOnPostTick;
        }

        private void TimerOnPostTick()
        {
            if (null == _gameManager.Player)
                return;

            bool isPlayerBoss = !_gameManager.Player.View.IsVictim;

            for (int i = 0; i < _view.Units.Length; i++)
            {
                var unit = _view.Units[i];

                // 1. Handle Employee Visibility (if player is Boss, hide slackers unless seen by the Boss)
                if (isPlayerBoss && unit != _gameManager.Player.View && unit.IsVictim)
                {
                    bool isSeen = _gameManager.Player.View.IsTargetVisible(unit);
                    
                    if (isSeen != unit.IsVissible)
                    {
                        unit.IsVissible = isSeen;
                        
                        // Play a cartoon smoke poof when visibility changes!
                        SpawnMagicPoof(unit.transform.position);
                    }
                }
                
                // 2. Handle Footprints & Sound Alerts when walking
                if (unit.AnimationType == AnimationType.Walk)
                {
                    // Spawn continuous footprints (every 0.35 seconds)
                    if (Time.time - _lastFootprintTime[i] > 0.35f)
                    {
                        _lastFootprintTime[i] = Time.time;
                        if (_isShowEffects && _gameManager.Player.View != unit)
                        {
                            CreateEffect(unit);
                        }
                    }

                    // Handle running wind trail (only if the unit doesn't have AIJuiceEffects, which manages its own trails)
                    if (_activeTrails[i] == null && _isShowEffects && _gameManager.Player.View != unit && unit.GetComponent<AIJuiceEffects>() == null)
                    {
                        var trailPrefab = Resources.Load<GameObject>("Prefabs/CFXR4 Wind Trails");
                        if (trailPrefab != null)
                        {
                            var trail = GameObject.Instantiate(trailPrefab, unit.transform.position, Quaternion.identity, unit.transform);
                            trail.transform.localPosition = new Vector3(0f, 0.1f, 0f);
                            trail.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                            _activeTrails[i] = trail;
                        }
                    }

                    if (_animations[i] != AnimationType.Walk)
                    {
                        if (unit.IsVictim)
                        {
                            _gameManager.FireSound(unit.transform.position);
                        }
                    }
                }
                else
                {
                    // Stop running trail when idle/stopped
                    if (_activeTrails[i] != null)
                    {
                        GameObject.Destroy(_activeTrails[i]);
                        _activeTrails[i] = null;
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

        private void SpawnMagicPoof(Vector3 position)
        {
            var poofPrefab = Resources.Load<GameObject>("Prefabs/CFXR Magic Poof");
            if (poofPrefab != null)
            {
                var poof = GameObject.Instantiate(poofPrefab, position + Vector3.up * 0.1f, Quaternion.identity);
                GameObject.Destroy(poof, 2.0f);
            }
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