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