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

            if (!_joystick.IsTouched)
            {
                _playerController.View.Idle();
                return;
            }

            _playerController.View.Walk();

            var joystickVector = _transform.forward * _joystick.Vertical + _transform.right * _joystick.Horizontal;

            var angle = Mathf.Atan2(_joystick.Horizontal, _joystick.Vertical) * Mathf.Rad2Deg;

            var deltaAngle = Mathf.Abs(Mathf.DeltaAngle(_transform.localEulerAngles.y, angle)) / 90f;
            deltaAngle = 1 - Mathf.Clamp01(deltaAngle);

            float angleLerpFactor = _config.GetValue(GameParam.AngleLerpFactor);

            angle = Mathf.LerpAngle(_transform.localEulerAngles.y, angle,
                Time.deltaTime * angleLerpFactor * joystickVector.sqrMagnitude);
            _transform.localEulerAngles = new Vector3(0f, angle, 0f);

            Vector3 direction = _transform.forward;
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

            var moveSpeed = speed * deltaAngle * joystickVector.magnitude;

            var newPosition = _transform.position + direction.normalized * Time.deltaTime * moveSpeed;

            _transform.position = newPosition;
        }
    }
}