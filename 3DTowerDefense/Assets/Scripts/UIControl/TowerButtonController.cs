using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerButtonController : MonoBehaviour
    {
        private int _lastIndex;
        private Image[] _towerButtons;
        private Sprite[] _defaultSprites;

        [SerializeField] private Sprite okSprite;

        private void Awake()
        {
            _lastIndex = -1;
            _towerButtons = new Image[transform.childCount];
            _defaultSprites = new Sprite[_towerButtons.Length];

            for (var i = 0; i < _towerButtons.Length; i++)
            {
                var index = i;
                transform.GetChild(i).GetComponent<Button>().onClick.AddListener(() => CheckButton(index));
                _towerButtons[i] = transform.GetChild(i).GetChild(0).GetComponent<Image>();
                _defaultSprites[i] = _towerButtons[i].sprite;
            }

            gameObject.SetActive(false);
        }

        private void CheckButton(int index)
        {
            if (_lastIndex != -1)
            {
                _towerButtons[_lastIndex].sprite = _defaultSprites[_lastIndex];
            }

            _lastIndex = index;
            _towerButtons[index].sprite = okSprite;
        }

        public void DefaultSprite()
        {
            if (_lastIndex == -1) return;
            _towerButtons[_lastIndex].sprite = _defaultSprites[_lastIndex];
            _lastIndex = -1;
        }
    }
}