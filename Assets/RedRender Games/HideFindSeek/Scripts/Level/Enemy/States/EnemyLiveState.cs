using Game.Core;
using Injection;
using UnityEngine;

namespace Game.Enemy.States
{
    public sealed class EnemyLiveState : EnemyState
    {
        [Inject] private Timer _timer;
        [Inject] private EnemyController _enemyController;
        [Inject] private Injector _injector;
        [Inject] private GameManager _gameManager;

        private Node _currentNode;
        private float _lastPushTime = 0f;

        public override void Initialize()
        {
            _enemyController.View.RestartNodes();
            _enemyController.View.NodeEntered += NodeEntered;
            _timer.POST_TICK += TimerOnPOST_TICK;

            _gameManager.UNIT_SOUND += OnHearUnit;
        }
        
        public override void Dispose()
        {
            if (null != _currentNode)
            {
                _currentNode.Dispose();
                _currentNode = null;
            }

            _enemyController.View.NodeEntered -= NodeEntered;
            _timer.POST_TICK -= TimerOnPOST_TICK;
            _gameManager.UNIT_SOUND -= OnHearUnit;
        }

        private void NodeEntered(Node node)
        {
            if (null != _currentNode)
            {
                _currentNode.Dispose();
            }

            _currentNode = node;
            _injector.Inject(node);
        }

        private void TimerOnPOST_TICK()
        {
            var result = _enemyController.GetSeenVictim();

            if (null != result)
            {
                _gameManager.Kill(result.GetComponent<UnitView>());
            }

            // Employee pushing mechanic: if this AI unit is an Employee, check for pushing!
            if (_enemyController.View.IsVictim)
            {
                CheckAndPushPlayer();
            }

            // Apply knockback/stumble movement and decay for this AI unit
            Vector3 knockback = _enemyController.View.KnockbackVelocity;
            if (knockback != Vector3.zero)
            {
                if (_enemyController.View.MeshAgent != null && _enemyController.View.MeshAgent.enabled)
                {
                    // If knockback is high, reduce AI pathfinding speed (stumbling effect)
                    if (knockback.magnitude > 1f)
                    {
                        _enemyController.View.MeshAgent.speed = _enemyController.Speed * 0.15f;
                    }
                    else
                    {
                        _enemyController.View.MeshAgent.speed = _enemyController.Speed;
                    }

                    _enemyController.View.MeshAgent.Move(knockback * Time.deltaTime);
                }

                // Decay knockback velocity
                _enemyController.View.KnockbackVelocity = Vector3.Lerp(knockback, Vector3.zero, Time.deltaTime * 6f);
                if (_enemyController.View.KnockbackVelocity.magnitude < 0.1f)
                {
                    _enemyController.View.KnockbackVelocity = Vector3.zero;
                    if (_enemyController.View.MeshAgent != null && _enemyController.View.MeshAgent.enabled)
                    {
                        _enemyController.View.MeshAgent.speed = _enemyController.Speed;
                    }
                }
            }
            
            if (null == _currentNode)
                return;

            if (_currentNode.IsCompleted)
            {
                _currentNode.Dispose();
                _currentNode = null;
                return;
            }

            _currentNode.Process();
        }

        private void CheckAndPushPlayer()
        {
            int difficulty = PlayerPrefs.GetInt("Difficulty", 1);
            if (difficulty == 0) return; // Easy mode: no pushing

            // 1. Push Player Boss (if player is Boss)
            if (_gameManager.Player != null && !_gameManager.Player.IsDied && !_gameManager.Player.View.IsVictim)
            {
                var playerView = _gameManager.Player.View;
                TryPushTarget(playerView, difficulty);
            }

            // 2. Push AI Boss (if Boss is AI and player is Employee)
            if (_gameManager.Player != null && _gameManager.Player.View.IsVictim)
            {
                foreach (var enemy in _gameManager.Enemies)
                {
                    if (enemy != null && enemy.View != null && !enemy.View.IsVictim && !enemy.View.IsDied())
                    {
                        TryPushTarget(enemy.View, difficulty);
                        break; // There is only one Boss
                    }
                }
            }
        }

        private void TryPushTarget(UnitView targetView, int difficulty)
        {
            Vector3 targetPos = targetView.transform.position;
            Vector3 myPos = _enemyController.View.transform.position;

            float distance = Vector3.Distance(myPos, targetPos);
            float cooldown = (difficulty == 2) ? 3.0f : 4.5f;

            if (distance < 2.0f && Time.time > _lastPushTime + cooldown)
            {
                // Check if employee is behind the target (boss)
                Vector3 targetForward = targetView.transform.forward;
                Vector3 targetToMe = (myPos - targetPos).normalized;
                float dot = Vector3.Dot(targetForward, targetToMe);

                if (dot < -0.4f) // behind the target (about 130 degree cone)
                {
                    _lastPushTime = Time.time;

                    // Apply knockback to the target!
                    Vector3 pushDirection = (targetPos - myPos).normalized;
                    pushDirection.y = 0;
                    pushDirection.Normalize();

                    // Apply push force based on difficulty
                    float pushForce = (difficulty == 2) ? 18f : 12f;
                    targetView.KnockbackVelocity = pushDirection * pushForce;

                    // Play push sound
                    if (Game.Audio.SoundManager.Instance != null)
                    {
                        Game.Audio.SoundManager.Instance.PlayPushSound();
                    }

                    Debug.Log($"[EnemyLiveState] AI Employee pushed target! TargetName={targetView.name}, Difficulty={difficulty}, Force={pushForce}");
                }
            }
        }

        private void OnHearUnit(Vector3 position)
        {
            if (_enemyController.View.IsVictim)
                return;

            var soundPosition = new Vector2(position.x, position.z);

            if (_enemyController.LastHearVictim == Vector2.zero)
            {
                _enemyController.LastHearVictim = soundPosition;
                return;
            }

            if (Vector2.Distance(soundPosition, _enemyController.Position) <
                Vector2.Distance(_enemyController.LastHearVictim, _enemyController.Position))
            {
                _enemyController.LastHearVictim = soundPosition;
            }
        }
    }
}