using Game.Config;
using Game.Core;
using Game.Core.UI;
using Injection;
using UnityEngine;
using Utils;

namespace Game
{
    public sealed class RescueUnitsManager : Mediator<LevelView>
    {
        [Inject] private Timer _timer;
        [Inject] private GameManager _gameManager;
        [Inject] private GameConfig _config;

        private float[] _lastRescueTime;
        private int _victimLayerMask;
        
        protected override void Show()
        {
            _lastRescueTime = new float[_view.Units.Length];
            _victimLayerMask = LayerUtils.GetVictimLayerMask();
            _timer.POST_TICK += TimerOnPOST_TICK;
        }

        protected override void Hide()
        {
            _timer.POST_TICK -= TimerOnPOST_TICK;
        }

        private void TimerOnPOST_TICK()
        {
            if (Time.frameCount % 2 == 0)
                return;

            var rescueReloadDuration = _config.GetValue(GameParam.RescueReloadDuration);

            for (int i = 0; i < _view.Units.Length; i++)
            {
                var unit = _view.Units[i];

                if (!unit.IsDied() || Time.time - _lastRescueTime[i] < rescueReloadDuration)
                    continue;

                var result = Physics.OverlapSphere(unit.transform.position, unit.Radius, _victimLayerMask);

                if (result.Length > 0)
                {
                    _lastRescueTime[i] = Time.time;

                    _gameManager.Rescue(unit);
                    _gameManager.CollectCoin(result[0].GetComponent<UnitView>(), (int)_config.GetValue(GameParam.CoinsByRescue));
                    _gameManager.FireSound(unit.transform.position);
                }
            }
        }
    }
}