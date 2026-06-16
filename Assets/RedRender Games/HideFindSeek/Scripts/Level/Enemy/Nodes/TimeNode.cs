using UnityEngine;

namespace Game.Enemy
{
    public class TimeNode : Node
    {
        [SerializeField] private float _time;

        private float _endTime;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _endTime = Time.time + _time;
        }

        public override void Dispose()
        {
        }

        public override void Process()
        {
            if (Time.time > _endTime)
            {
                Completed();
                NextNode();
            }
        }

        protected virtual void Completed()
        {
        }

        public override string ToString()
        {
            return base.ToString() + " Time: " + _time;
        }
    }
}