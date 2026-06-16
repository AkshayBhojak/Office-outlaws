using Injection;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemy
{
    public sealed class GoToLastSeePlayerPositionNode : Node
    {
        [Inject] private EnemyController _enemyController;
        private NavMeshAgent _meshAgent;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            _meshAgent = _enemyController.View.MeshAgent;

            var targetPoint = _enemyController.LastHearVictim;

            _meshAgent.SetDestination(new Vector3(targetPoint.x, 0, targetPoint.y));
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
    }
}