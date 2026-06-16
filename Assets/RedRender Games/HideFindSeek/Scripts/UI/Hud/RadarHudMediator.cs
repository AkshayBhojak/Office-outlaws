using Game.Core;
using Game.Core.UI;
using Injection;
using UnityEngine;

namespace Game.UI
{
    public sealed class RadarHudMediator : Mediator<GameHud>
    {
        [Inject] private Timer _timer;
        [Inject] private LevelView _levelView;

        private float _minX, _maxX, _minY, _maxY;
        private UnityEngine.Camera _camera;

        protected override void Show()
        {
            _camera = UnityEngine.Camera.main;

            _minX = (_view.TrRadarIcon.sizeDelta.x / 2f);
            _maxX = (Screen.width - (_view.TrRadarIcon.sizeDelta.x / 2f));

            _minY = (_view.TrRadarIcon.sizeDelta.y / 2f);
            _maxY = (Screen.height - (_view.TrRadarIcon.sizeDelta.y / 2f));

            _timer.POST_TICK += TimerOnPOST_TICK;
        }

        protected override void Hide()
        {
            _view.TrRadarIcon.gameObject.SetActive(false);
            _timer.POST_TICK -= TimerOnPOST_TICK;
        }

        private void TimerOnPOST_TICK()
        {
            Vector2 targetPos = _camera.WorldToScreenPoint(_levelView.Units[0].transform.position);

            bool isOutside =
                !IsBetweenInclusive(targetPos.x, _minX, _maxX) ||
                !IsBetweenInclusive(targetPos.y, _minY, _maxY);

            _view.TrRadarIcon.gameObject.SetActive(isOutside);


            if (!isOutside)
                return;

            Vector2 position;
            Vector2 screenCenter = new Vector2(Screen.width * .5f, Screen.height * .5f);

            if (!LineIntersection.FindIntersection(screenCenter, targetPos,
                    Screen.width, 0, Screen.width, Screen.height, true, out position) &&
                !LineIntersection.FindIntersection(screenCenter, targetPos,
                    0, 0, 0, Screen.height, true, out position) &&
                !LineIntersection.FindIntersection(screenCenter, targetPos,
                    0, 0, Screen.width, 0, true, out position) &&
                !LineIntersection.FindIntersection(screenCenter, targetPos,
                    0, Screen.height, Screen.width, Screen.height, true, out position))
            {
                position = targetPos;
            }

            position.x = Mathf.Clamp(position.x, _minX, _maxX);
            position.y = Mathf.Clamp(position.y, _minY, _maxY);

            _view.TrRadarIcon.position = position;

            Vector2 diff = new Vector2(targetPos.x, targetPos.y) - position;

            diff.Normalize();
            float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            _view.TrRadarIcon.transform.rotation = Quaternion.Euler(0f, 0f, rotZ - 90f);
        }

        private bool IsBetweenInclusive(float value, float bound1, float bound2)
        {
            return value >= bound1 && value <= bound2;
        }
    }
}