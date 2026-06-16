using Game.Config;
using Injection;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace Game.Enemy
{
    public sealed class PathToRandomPositionNode : Node
    {
        [Inject] private GameModel _model;
        [Inject] private EnemyController _enemyController;
        [Inject] private LevelView _levelView;
        [Inject] private GameConfig _config;

        private NavMeshAgent _meshAgent;
        private float _previousDistanceToEnemy;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _meshAgent = _enemyController.View.MeshAgent;

            var position = new Vector3(_enemyController.LastHearVictim.x, 0, _enemyController.LastHearVictim.y);

            if (position == Vector3.zero)
            {
                position.x = MathUtil.RandomSystem(_model.LevelBounds.x, _model.LevelBounds.z);
                position.z = MathUtil.RandomSystem(_model.LevelBounds.y, _model.LevelBounds.w);
            }

            _enemyController.LastHearVictim = Vector2.zero;

            _meshAgent.SetDestination(position);
            _meshAgent.isStopped = false;

            _enemyController.View.Walk();
        }

        public override void Dispose()
        {
            _meshAgent.isStopped = true;
            _meshAgent = null;
            _enemyController.View.Idle();
        }

        public override void Process()
        {
            var distanceToEnemy = Vector2.Distance(_enemyController.Position, _levelView.Units[0].Position);
            
            if (_enemyController.View.IsVictim && 
                distanceToEnemy < _config.GetValue(GameParam.MinDistanceToEnemy) && distanceToEnemy < _previousDistanceToEnemy)
            {
                var position = Vector3.zero;
                position.x = MathUtil.RandomSystem(_model.LevelBounds.x, _model.LevelBounds.z);
                position.z = MathUtil.RandomSystem(_model.LevelBounds.y, _model.LevelBounds.w);
                _meshAgent.SetDestination(position);
            }

            _previousDistanceToEnemy = distanceToEnemy;

            if (_meshAgent.remainingDistance <= 0 && !_meshAgent.pathPending)
            {
                NextNode();
            }
        }
    }
}