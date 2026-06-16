using UnityEngine;

namespace Game.Core.UI
{
    public abstract class Mediator
    {
        public abstract void Unmediate();
    }

    public abstract class Mediator<T> : Mediator where T : MonoBehaviour
    {
        protected T _view;

        public void Mediate(T view)
        {
            _view = view;
            Show();
        }

        public override void Unmediate()
        {
            Hide();
            _view = null;
        }

        protected abstract void Show();
        protected abstract void Hide();
    }
}