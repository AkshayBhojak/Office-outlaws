using TMPro;
using UnityEngine;

namespace Game.UI
{
    public sealed class UnitsHolderHud : MonoBehaviour
    {
        [SerializeField] private GameObject[] _greenHolders;
        [SerializeField] private GameObject[] _yellowHolders;
        [SerializeField] private TextMeshProUGUI _txtOnlyOneLeft;

        private int _count;
        private int _maxCount;
        private bool _isGreen;

        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;

                _txtOnlyOneLeft.gameObject.SetActive((_count + 1 == _maxCount) && _isGreen);

                var holders = (_isGreen) ? _greenHolders : _yellowHolders;

                for (int i = 0; i < holders.Length; i++)
                {
                    holders[i].SetActive(i < _count);
                }
            }
        }

        public void SwitchToMode(bool isGreen, int maxCount)
        {
            _isGreen = false;
            Count = 0;
            _isGreen = true;
            Count = 0;

            _isGreen = isGreen;
            _maxCount = maxCount;

            for (int i = 0; i < _greenHolders.Length; i++)
            {
                _greenHolders[i].transform.parent.gameObject.SetActive(i < maxCount);
            }
        }
    }
}