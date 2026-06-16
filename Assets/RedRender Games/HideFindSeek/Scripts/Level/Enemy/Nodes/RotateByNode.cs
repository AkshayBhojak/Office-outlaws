using Injection;
using UnityEngine;

namespace Game.Enemy
{
    public sealed class RotateByNode : Node
    {
        [SerializeField] private float _angle;

        [Inject] private EnemyController _enemyController;

        private float _time;
        private float _startAngle;
        private float _startTime;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            _startAngle = Animator.transform.localEulerAngles.y;
            _startTime = Time.time;
            _time = _angle / _enemyController.RotationSpeed;
            _enemyController.View.Walk();
        }

        public override void Dispose()
        {
            _enemyController.View.Idle();
        }

        public override void Process()
        {
            var factor = (Time.time - _startTime) / _time;

            float angle = Mathf.Lerp(_startAngle, _startAngle + _angle, factor);
            Animator.transform.localEulerAngles = new Vector3(0, angle, 0);

            if (factor >= 1)
            {
                NextNode();
            }
        }

        public override string ToString()
        {
            return base.ToString() + " Angle: " + _angle;
        }
    }
}