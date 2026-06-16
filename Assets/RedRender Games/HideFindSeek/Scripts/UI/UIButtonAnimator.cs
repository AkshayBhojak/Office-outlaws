using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Game.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Vector3 _originalScale;
        private Button _button;

        private void Awake()
        {
            _originalScale = transform.localScale;
            if (_originalScale == Vector3.zero)
            {
                _originalScale = Vector3.one;
            }
            _button = GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            transform.DOScale(_originalScale * 1.1f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            transform.DOScale(_originalScale, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            transform.DOScale(_originalScale * 0.95f, 0.1f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            transform.DOScale(_originalScale * 1.1f, 0.1f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        private void OnDisable()
        {
            transform.localScale = _originalScale;
        }
    }
}
