using UnityEngine;

namespace Game
{
    public enum AnimationType
    {
        Idle,
        Walk,
        Died
    }

    public abstract class BaseUnitView : MonoBehaviour
    {
        public Vector2 Position
        {
            get
            {
                return new Vector2(transform.position.x, transform.position.z);
            }
            set
            {
                transform.position = new Vector3(value.x, 0, value.y);
            }
        }

        public float Rotation
        {
            get
            {
                return transform.localEulerAngles.y;
            }
            set
            {
                transform.localEulerAngles = new Vector3(0, value, 0);
            }
        }
    }

    public abstract class AnimationUnitView : BaseUnitView
    {
        [SerializeField] private Animator _animator;

        private AnimationType _animationType;

        public AnimationType AnimationType
        {
            get
            {
                return _animationType;
            }
        }

        public bool IsDied()
        {
            return _animationType == AnimationType.Died;
        }

        public virtual void Idle()
        {
            PlayAnimation(AnimationType.Idle);
        }

        public void Walk()
        {
            PlayAnimation(AnimationType.Walk);
        }

        public virtual void Die()
        {
            PlayAnimation(AnimationType.Died);
        }

        private void PlayAnimation(AnimationType animationType)
        {
            if (_animationType == animationType)
                return;

            _animationType = animationType;
            var nameHash = Animator.StringToHash(_animationType.ToString());

            _animator.Play(nameHash);

            if (_animator.HasState(1, nameHash))
            {
                _animator.Play(nameHash, 1);
            }
            _animator.Update(0);
        }
    }
}