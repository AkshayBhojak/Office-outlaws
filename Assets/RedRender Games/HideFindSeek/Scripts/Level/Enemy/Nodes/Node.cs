using System;
using UnityEngine;

namespace Game.Enemy
{
    public abstract class Node : StateMachineBehaviour, IDisposable
    {
        protected Animator Animator { get; private set; }

        public bool IsCompleted
        {
            get { return Animator.speed > 0; }
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Animator = animator;
            Animator.speed = 0;
            Animator.GetComponent<UnitView>().FireNodeEnter(this);
        }

        public abstract void Dispose();

        protected void NextNode()
        {
            Animator.speed = 10000;
        }

        public abstract void Process();

        public override string ToString()
        {
            return string.Format("({0})", GetType().Name);
        }
    }
}