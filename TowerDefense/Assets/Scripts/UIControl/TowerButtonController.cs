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
                transform.GetChild(i).GetComponent<Button>().onClick.AddListener(() => CheckButtonTest(index));
                _towerButtons[i] = transform.GetChild(i).GetChild(0).GetComponent<Image>();
                _defaultSprites[i] = _towerButtons[i].sprite;
            }
        }

        private void CheckButton(int index)
        {
            if (_lastIndex != -1)
            {
                _towerButtons[_lastIndex].sprite = _defaultSprites[_lastIndex];
                return;
            }

            _lastIndex = index;
            _towerButtons[index].sprite = okSprite;
        }

        private void CheckButtonTest(int index)
        {
            if (_lastIndex != index && _lastIndex != -1)
            {
                _towerButtons[_lastIndex].sprite = _defaultSprites[_lastIndex];
                _lastIndex = index;
                _towerButtons[_lastIndex].sprite = okSprite;
                return;
            }
            
            _lastIndex = index;
            if (_towerButtons[_lastIndex].sprite == okSprite)
            {
                _towerButtons[_lastIndex].sprite = _defaultSprites[_lastIndex];
                return;
            }

            _towerButtons[_lastIndex].sprite = okSprite;
        }

        public void SetDefaultSprite()
        {
            if (_lastIndex == -1) return;
            _towerButtons[_lastIndex].sprite = _defaultSprites[_lastIndex];
            _lastIndex = -1;
        }
    }
}