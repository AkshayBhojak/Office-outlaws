using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.UI
{
    public sealed class EndGameHud : MonoBehaviour
    {
        [SerializeField] 
        public Button BtnRestart;
        [SerializeField]
        public TextMeshProUGUI TxtRestartLabel;

        private TextMeshProUGUI _txtResultTitle;

        public string RestartLabel
        {
            set
            {
                TxtRestartLabel.text = value;
            }
        }

        public void SetupResultTitle(bool isWin)
        {
            if (_txtResultTitle == null)
            {
                GameObject titleGo = new GameObject("ResultTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
                titleGo.transform.SetParent(transform, false);

                _txtResultTitle = titleGo.GetComponent<TextMeshProUGUI>();
                _txtResultTitle.font = TxtRestartLabel.font;
                _txtResultTitle.fontSize = 90;
                _txtResultTitle.alignment = TextAlignmentOptions.Center;

                RectTransform rect = titleGo.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, 250f);
                rect.sizeDelta = new Vector2(800f, 200f);
            }

            if (isWin)
            {
                _txtResultTitle.text = "<color=#22ff66>PROMOTED!</color>";
            }
            else
            {
                _txtResultTitle.text = "<color=#ff3333>FIRED!</color>";
            }

            // Animate Victory/Defeat text
            _txtResultTitle.transform.localScale = Vector3.zero;
            _txtResultTitle.transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBounce).SetUpdate(true);

            // Bouncing/scaling effect
            _txtResultTitle.transform.DOScale(Vector3.one * 1.1f, 1.2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        private Button _btnRewarded;
        private TextMeshProUGUI _txtRewardedLabel;
        private Vector2? _originalRestartPos;

        public void SetupRewardedButton(string buttonText, System.Action onWatchAdClicked)
        {
            RectTransform restartRect = BtnRestart.GetComponent<RectTransform>();
            if (!_originalRestartPos.HasValue)
            {
                _originalRestartPos = restartRect.anchoredPosition;
            }

            // Move BtnRestart up by 30 units to make space
            restartRect.anchoredPosition = _originalRestartPos.Value + new Vector2(0f, 30f);

            if (_btnRewarded != null)
            {
                _btnRewarded.gameObject.SetActive(true);
                if (_txtRewardedLabel != null)
                {
                    _txtRewardedLabel.text = buttonText;
                }

                // Update position of the rewarded button relative to the updated restart position
                _btnRewarded.GetComponent<RectTransform>().anchoredPosition = restartRect.anchoredPosition + new Vector2(0f, -180f);

                _btnRewarded.onClick.RemoveAllListeners();
                _btnRewarded.onClick.AddListener(() => onWatchAdClicked?.Invoke());
                return;
            }

            // Duplicate the BtnRestart to copy all visual styles!
            GameObject rewardedGo = Instantiate(BtnRestart.gameObject, BtnRestart.transform.parent);
            rewardedGo.name = "BtnRewarded";

            // Position it below BtnRestart
            RectTransform rewardedRect = rewardedGo.GetComponent<RectTransform>();
            rewardedRect.anchoredPosition = restartRect.anchoredPosition + new Vector2(0f, -180f);

            _btnRewarded = rewardedGo.GetComponent<Button>();
            _btnRewarded.onClick.RemoveAllListeners();
            _btnRewarded.onClick.AddListener(() => onWatchAdClicked?.Invoke());

            _txtRewardedLabel = rewardedGo.GetComponentInChildren<TextMeshProUGUI>();
            if (_txtRewardedLabel != null)
            {
                _txtRewardedLabel.text = buttonText;
                _txtRewardedLabel.fontSize = TxtRestartLabel.fontSize * 0.8f;
            }

            // Add button animator if not present
            if (rewardedGo.GetComponent<UIButtonAnimator>() == null)
            {
                rewardedGo.AddComponent<UIButtonAnimator>();
            }
        }

        public void HideRewardedButton()
        {
            if (_btnRewarded != null)
            {
                _btnRewarded.gameObject.SetActive(false);
            }

            if (_originalRestartPos.HasValue)
            {
                RectTransform restartRect = BtnRestart.GetComponent<RectTransform>();
                restartRect.anchoredPosition = _originalRestartPos.Value;
            }
        }
    }
}