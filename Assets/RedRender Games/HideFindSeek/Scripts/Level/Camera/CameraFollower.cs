using DG.Tweening;
using UnityEngine;

namespace Game.Camera
{
    public sealed class CameraFollower : MonoBehaviour
    {
        public GameObject player;
        public float lerpFactor;
        public float Radius = 50f;
        public float ZoomDuration;

        Vector3 difference;
        Vector3 startPos;
        Vector3 _clampedPosition;
        Transform _transform;
        private Vector3 _defaultPosition;

        private float Zoom
        {
            get
            {
                return UnityEngine.Camera.main.fieldOfView;
            }
            set
            {
                UnityEngine.Camera.main.fieldOfView = value;
            }
        }

        private void Awake()
        {
            _defaultPosition = transform.position;
        }

        public void ResetToDefaultPosition()
        {
            transform.position = _defaultPosition;
        }

        public void SetTarget(GameObject target)
        {
            enabled = null != target;

            if (null == target)
                return;

            transform.position = _defaultPosition;

            player = target;

            _transform = transform;
            _clampedPosition = _transform.position;

            startPos = transform.position + player.transform.position;
            transform.position = startPos;
            difference = player.transform.position - transform.position;
        }

        private void Update()
        {
            Vector3 posEnd = player.transform.position - difference;

            _transform.position = Vector3.Lerp(_transform.position, posEnd, lerpFactor);
            _clampedPosition = Vector3.ClampMagnitude(_transform.position, Radius);
            transform.position = Vector3.Lerp(_transform.position, _clampedPosition, lerpFactor);
        }

        public void ZoomTo(float zoom, float duration)
        {
            DOTween.Kill(this);
            DOTween.To(() => Zoom, x => Zoom = x, zoom, duration * ZoomDuration).SetTarget(this);

            if (Mathf.Approximately(duration, 0))
            {
                Zoom = zoom;
            }
        }
    }
}