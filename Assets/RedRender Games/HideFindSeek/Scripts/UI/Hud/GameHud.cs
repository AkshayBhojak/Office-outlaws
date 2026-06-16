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
}