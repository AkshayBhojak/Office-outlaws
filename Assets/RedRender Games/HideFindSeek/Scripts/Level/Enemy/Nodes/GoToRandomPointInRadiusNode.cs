using Injection;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace Game.Enemy
{
    public sealed class GoToRandomPointInRadiusNode : Node
    {
        [SerializeField] private float _radius;

        [Inject] private EnemyController _enemyController;
        private NavMeshAgent _meshAgent;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            var targetPoint = _enemyController.View.transform.position;
            var angle = MathUtil.RandomSystem(0, Mathf.PI * 2);
            targetPoint.x += Mathf.Sin(angle) * _radius;
            targetPoint.z += Mathf.Cos(angle) * _radius;

            _meshAgent = _enemyController.View.MeshAgent;

            _meshAgent.SetDestination(targetPoint);
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
            if (_meshAgent.remainingDistance <= 0 && !_meshAgent.pathPending)
            {
                NextNode();
            }
        }

        public override string ToString()
        {
            return base.ToString() + " Radius: " + _radius;
        }
    }
}