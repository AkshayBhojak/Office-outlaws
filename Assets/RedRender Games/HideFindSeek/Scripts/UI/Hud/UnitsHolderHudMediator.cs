using Game.Core.UI;
using Injection;

namespace Game.UI
{
    public sealed class UnitsHolderHudMediator : Mediator<UnitsHolderHud>
    {
        [Inject] private GameManager _gameManager;
        [Inject] private LevelView _levelView;

        private readonly bool _isHided;

        public UnitsHolderHudMediator(bool isHided)
        {
            _isHided = isHided;
        }

        protected override void Show()
        {
            _view.SwitchToMode(!_isHided, _levelView.Units.Length - 1);

            _gameManager.UNIT_KILLED += OnUnitKilled;
            _gameManager.UNIT_RESCUED += OnUnitRescued;
        }

        protected override void Hide()
        {
            _gameManager.UNIT_KILLED -= OnUnitKilled;
            _gameManager.UNIT_RESCUED -= OnUnitRescued;
        }

        private void OnUnitKilled(UnitView obj)
        {
            _view.Count++;
        }

        private void OnUnitRescued(UnitView obj)
        {
            _view.Count--;
        }
    }
}