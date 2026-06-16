using Game.UI;
using UnityEngine;

namespace Game.States
{
    public sealed class GamePlayHideState : BaseGamePlayState
    {
        public override void Initialize()
        {
            base.Initialize();

            for (int i = 2; i < _levelView.Units.Length; i++)
            {
                _gameManager.CreateEnemy(_levelView.Units[i], true);
            }

            _gameManager.CreatePlayer(_levelView.Units[1], true);
            _gameView.CameraFollower.SetTarget(_gameManager.Player.View.gameObject);

            AddMediator(new GameHudMediator(true), _gameView.GameHud);
            AddMediator(new UnitsHolderHudMediator(true), _gameView.UnitsHolderHud);
            AddMediator(new RadarHudMediator(), _gameView.GameHud);

            AddMediator(new HearStepsManager(false), _levelView);

            _timer.ONE_SECOND_TICK += TimerOnONE_SECOND_TICK;
        }

        public override void Dispose()
        {
            base.Dispose();
            _gameView.CameraFollower.SetTarget(null);
            _timer.ONE_SECOND_TICK -= TimerOnONE_SECOND_TICK;
            _gameManager.Dispose();
        }

        private void TimerOnONE_SECOND_TICK()
        {
            if (Time.time > _startTime)
            {
                _gameManager.CreateEnemy(_levelView.Units[0], false);
                _startTime = float.MaxValue;
            }

            if (_gameManager.Player.IsDied || _gameModel.IsTimeOut)
            {
                _timer.ONE_SECOND_TICK -= TimerOnONE_SECOND_TICK;

                _gameView.GameHud.gameObject.SetActive(false);

                bool isWin = !_gameManager.Player.IsDied;

                if (isWin)
                {
                    _gameManager.Player.View.Idle();
                    _gameManager.Player.Dispose();
                }
                
                AddMediator(new EndGameHudMediator(isWin), _gameView.EndGameHud);
            }
        }

        public void Revive()
        {
            if (_gameManager.Player != null)
            {
                // 1. Remove the cage from the player slacker
                if (_gameManager.Player.View != null)
                {
                    _gameManager.UNIT_RESCUED.SafeInvoke(_gameManager.Player.View);
                }

                // 2. Restore the player to live state (Employee slacker)
                _gameManager.Player.Live(true);

                // 3. Teleport player back to the center (0, y, 0) to prevent immediate re-capture
                var view = _gameManager.Player.View;
                if (view != null)
                {
                    view.transform.position = new Vector3(0f, view.transform.position.y, 0f);
                }
            }

            // 4. Reactivate HUD
            _gameView.GameHud.gameObject.SetActive(true);

            // 5. Re-subscribe to timer tick listener
            _timer.ONE_SECOND_TICK -= TimerOnONE_SECOND_TICK; // Safely unsubscribe first
            _timer.ONE_SECOND_TICK += TimerOnONE_SECOND_TICK;

            // 6. Find the GameHudMediator and resume it
            foreach (var mediator in _mediators)
            {
                if (mediator is GameHudMediator hudMediator)
                {
                    hudMediator.ResumeTimer(30f);
                    break;
                }
            }
        }
    }
}