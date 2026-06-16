using Injection;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemy
{
    public sealed class PathToNode : Node
    {
        [SerializeField] private Vector3 _position;

        [Inject] private EnemyController _enemyController;
        private NavMeshAgent _meshAgent;

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _meshAgent = _enemyController.View.MeshAgent;

            _meshAgent.SetDestination(_position);
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
            return base.ToString() + " Position: " + _position;
        }
    }
}