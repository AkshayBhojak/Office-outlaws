using Game.Config;
using Game.Core;
using Injection;
using TPSShooter;
using UnityEngine;

namespace Game.Player.States
{
    public sealed class PlayerLiveState : PlayerState
    {
        [Inject] private Timer _timer;
        [Inject] private GameView _view;
        [Inject] private PlayerController _playerController;
        [Inject] private GameConfig _config;
        [Inject] private GameManager _gameManager;

        private Joystick _joystick;
        private Transform _transform;
        private float _lastPushTime = 0f;
        
        public PlayerLiveState()
        {
        }

        public override void Initialize()
        {
            _joystick = _view.GameHud.Joystick;
            _transform = _playerController.View.transform;
            _timer.TICK += TimerOnTICK;
        }

        public override void Dispose()
        {
            _timer.TICK -= TimerOnTICK;
        }

        private void TimerOnTICK()
        {
            var result = _playerController.RaycastVictim();

            if (null != result)
            {
                var enemyView = result.GetComponent<UnitView>();
                _gameManager.Kill(enemyView);
            }

            // Player Employee pushing mechanic: if player is Employee, check for pushing the Boss AI!
            if (_playerController.View.IsVictim)
            {
                CheckAndPushBossAI();
            }

            Vector3 knockback = _playerController.View.KnockbackVelocity;

            if (!_joystick.IsTouched && knockback.magnitude < 0.1f)
            {
                _playerController.View.Idle();
                return;
            }

            Vector3 direction = _transform.forward;
            float moveSpeed = 0f;

            if (_joystick.IsTouched)
            {
                _playerController.View.Walk();

                var joystickVector = _transform.forward * _joystick.Vertical + _transform.right * _joystick.Horizontal;

                var angle = Mathf.Atan2(_joystick.Horizontal, _joystick.Vertical) * Mathf.Rad2Deg;

                var deltaAngle = Mathf.Abs(Mathf.DeltaAngle(_transform.localEulerAngles.y, angle)) / 90f;
                deltaAngle = 1 - Mathf.Clamp01(deltaAngle);

                float angleLerpFactor = _config.GetValue(GameParam.AngleLerpFactor);

                angle = Mathf.LerpAngle(_transform.localEulerAngles.y, angle,
                    Time.deltaTime * angleLerpFactor * joystickVector.sqrMagnitude);
                _transform.localEulerAngles = new Vector3(0f, angle, 0f);

                direction = _transform.forward;
                float speed = _config.GetValue(GameParam.Speed);
                
                // Scale player speed based on difficulty
                int difficulty = PlayerPrefs.GetInt("Difficulty", 1);
                if (difficulty == 0) // Easy
                {
                    speed *= 1.15f; // +15% speed
                }
                else if (difficulty == 2) // Hard
                {
                    speed *= 0.9f; // -10% speed
                }

                moveSpeed = speed * deltaAngle * joystickVector.magnitude;

                if (knockback.magnitude > 1f)
                {
                    // Reduce player's steer control significantly while stumbling!
                    moveSpeed *= 0.15f;
                }
            }
            else
            {
                _playerController.View.Idle();
            }

            var newPosition = _transform.position + (direction.normalized * moveSpeed + knockback) * Time.deltaTime;
            _transform.position = newPosition;

            // Decay knockback velocity over time
            if (knockback != Vector3.zero)
            {
                _playerController.View.KnockbackVelocity = Vector3.Lerp(knockback, Vector3.zero, Time.deltaTime * 6f);
                if (_playerController.View.KnockbackVelocity.magnitude < 0.1f)
                {
                    _playerController.View.KnockbackVelocity = Vector3.zero;
                }
            }
        }

        private void CheckAndPushBossAI()
        {
            int difficulty = PlayerPrefs.GetInt("Difficulty", 1);
            if (difficulty == 0) return; // Easy mode: no pushing

            // Find the Boss AI
            foreach (var enemy in _gameManager.Enemies)
            {
                if (enemy != null && enemy.View != null && !enemy.View.IsVictim && !enemy.View.IsDied())
                {
                    var bossView = enemy.View;
                    Vector3 bossPos = bossView.transform.position;
                    Vector3 myPos = _playerController.View.transform.position;

                    float distance = Vector3.Distance(myPos, bossPos);
                    float cooldown = (difficulty == 2) ? 3.0f : 4.5f;

                    if (distance < 2.0f && Time.time > _lastPushTime + cooldown)
                    {
                        // Check if player is behind the boss AI
                        Vector3 bossForward = bossView.transform.forward;
                        Vector3 bossToMe = (myPos - bossPos).normalized;
                        float dot = Vector3.Dot(bossForward, bossToMe);

                        if (dot < -0.4f) // behind the boss (about 130 degree cone)
                        {
                            _lastPushTime = Time.time;

                            // Apply knockback to the Boss AI!
                            Vector3 pushDirection = (bossPos - myPos).normalized;
                            pushDirection.y = 0;
                            pushDirection.Normalize();

                            // Apply push force based on difficulty
                            float pushForce = (difficulty == 2) ? 18f : 12f;
                            bossView.KnockbackVelocity = pushDirection * pushForce;

                            // Play push sound
                            if (Game.Audio.SoundManager.Instance != null)
                            {
                                Game.Audio.SoundManager.Instance.PlayPushSound();
                            }

                            Debug.Log($"[PlayerLiveState] Player Employee pushed the Boss AI! Difficulty={difficulty}, Force={pushForce}");
                        }
                    }
                    break; // There is only one Boss
                }
            }
        }
    }
}