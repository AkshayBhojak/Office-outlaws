using DG.Tweening;
using UnityEngine;
using TMPro;
using TPSShooter;
using UnityEngine.UI;

public sealed class GameHud : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _txtTimeToHide;
    [SerializeField]
    private TextMeshProUGUI _txtTimeToHideLabel;
    [SerializeField]
    private TimeCounterColors _colors;
    [SerializeField]
    private TextMeshProUGUI _txtTimeLeft;
    [SerializeField]
    private Image _imgTimeLeft;
    [SerializeField]
    private TextMeshProUGUI _txtCoins;

    [SerializeField]
    public Joystick Joystick;

    [SerializeField] 
    public RectTransform TrRadarIcon;

    private int _roundedTime;
    private int _coins;
    private float _time;

    public string TimeToHideLabelText
    {
        set
        {
            _txtTimeToHideLabel.text = value;
        }
    }

    public int Coins
    {
        get
        {
            return _coins;
        }
        set
        {
            DOTween.Kill(_txtCoins.transform.parent);
            _txtCoins.transform.localScale = Vector3.one;

            _coins = value;
            _txtCoins.text = value.ToString();

            if (value <= 0)
                return;

            _txtCoins.transform.parent.DOScale(Vector3.one * 1.2f, .25f).SetLoops(2, LoopType.Yoyo);
        }
    }

    public float TimeToHide
    {
        get
        {
            return _time;
        }
        set
        {
            _time = value;

            _txtTimeToHide.gameObject.SetActive(Mathf.CeilToInt(value) >= 0);
            _txtTimeToHideLabel.gameObject.SetActive(Mathf.CeilToInt(value) >= 0);

            if (_roundedTime == Mathf.CeilToInt(value))
                return;

            _roundedTime = Mathf.CeilToInt(value);

            if (!_txtTimeToHide.gameObject.activeInHierarchy)
                return;

            DOTween.Kill(_txtTimeToHide.transform);
            _txtTimeToHide.transform.localScale = Vector3.one;

            _txtTimeToHide.text = _roundedTime.ToString();
            _txtTimeToHide.transform.DOScale(Vector3.one * 2, .25f).SetLoops(2, LoopType.Yoyo);

            if (_roundedTime == 0)
            {
                _txtTimeToHide.DOFade(0, 1f);
                _txtTimeToHideLabel.DOFade(0, 1f);
            }
            else
            {
                _txtTimeToHide.alpha = 1;
                _txtTimeToHideLabel.alpha = 1;
            }
        }
    }

    public bool SetTimeLeft(float startTime, float duration, bool isGreenTimeRemain)
    {
        _txtTimeLeft.gameObject.SetActive(duration > 0);
        _imgTimeLeft.transform.parent.gameObject.SetActive(duration > 0);

        if (duration <= 0)
            return false;

        var time = startTime + duration - Time.time;
        time = Mathf.Max(time, 0);

        _txtTimeLeft.text = string.Format("0:{0:D2}", Mathf.CeilToInt(time));

        var color = _colors.regular;

        _imgTimeLeft.fillAmount = time / duration;
        
        if (Mathf.CeilToInt(time) <= 10)
        {
            color = (isGreenTimeRemain) ? _colors.green : _colors.pink;
        }

        _imgTimeLeft.color = color;
        _txtTimeLeft.color = color;

        return time <= 0;
    }

    private GameObject _tutorialPopup;

    public void ShowTutorialPopup(bool isHideMode)
    {
        if (_tutorialPopup == null)
        {
            _tutorialPopup = new GameObject("TutorialPopup", typeof(RectTransform), typeof(Image));
            _tutorialPopup.transform.SetParent(transform, false);

            RectTransform rect = _tutorialPopup.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 150f);
            rect.sizeDelta = new Vector2(800f, 300f);

            Image bg = _tutorialPopup.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f); // Semi-transparent black

            GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(_tutorialPopup.transform, false);

            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 50f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
        }

        TextMeshProUGUI tmpText = _tutorialPopup.GetComponentInChildren<TextMeshProUGUI>();
        
        if (isHideMode)
        {
            tmpText.text = "<color=#ffcc00>EMPLOYEE MODE</color>\n\nHide from the Boss!\nCollect coins and survive!";
        }
        else
        {
            tmpText.text = "<color=#ff3333>BOSS MODE</color>\n\nCatch all employees\nbefore time runs out!";
        }

        _tutorialPopup.SetActive(true);
        _tutorialPopup.transform.localScale = Vector3.zero;

        // Animate in and out
        Sequence seq = DOTween.Sequence();
        seq.Append(_tutorialPopup.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        seq.AppendInterval(3.5f);
        seq.Append(_tutorialPopup.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack));
        seq.OnComplete(() => _tutorialPopup.SetActive(false));
        seq.SetUpdate(true);
        seq.SetTarget(this);
    }

    public System.Action OnMenuClicked;
    private Button _btnMenu;

    public void SetupMenuButton()
    {
        if (_btnMenu != null) return;

        // Create a new UI Button for "MENU" at the top-left
        GameObject btnGo = new GameObject("BtnMenu", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(transform, false);

        RectTransform rect = btnGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(50f, -80f);
        rect.sizeDelta = new Vector2(280f, 140f); // Increased size from 160x80 to 280x140

        // Apply Custom Sprite
        Image img = btnGo.GetComponent<Image>();
        img.sprite = Resources.Load<Sprite>("UI/BacktoManimenu");
        img.color = Color.white;

        // Button logic
        _btnMenu = btnGo.GetComponent<Button>();
        _btnMenu.onClick.AddListener(() =>
        {
            if (OnMenuClicked != null)
                OnMenuClicked.Invoke();
        });

        // Small animator
        if (btnGo.GetComponent<Game.UI.UIButtonAnimator>() == null)
            btnGo.AddComponent<Game.UI.UIButtonAnimator>();
    }
}