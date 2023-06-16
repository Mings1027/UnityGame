using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerEditButtonController : MonoBehaviour
    {
        private int _lastIndex;
        private Image[] _editButtons;
        private Sprite[] _defaultSprites;

        [SerializeField] private Sprite okSprite;

        private void Awake()
        {
            _lastIndex = -1;
            _editButtons = new Image[transform.childCount];
            _defaultSprites = new Sprite[_editButtons.Length];

            for (var i = 0; i < _editButtons.Length; i++)
            {
                var index = i;
                transform.GetChild(i).GetComponent<Button>().onClick.AddListener(() => CheckButton(index));
                _editButtons[i] = transform.GetChild(i).GetChild(0).GetComponent<Image>();
                _defaultSprites[i] = _editButtons[i].sprite;
            }

            gameObject.SetActive(false);
        }

        private void CheckButton(int index)
        {
            if (_lastIndex != -1)
            {
                _editButtons[_lastIndex].sprite = _defaultSprites[_lastIndex];
            }

            _lastIndex = index;
            _editButtons[index].sprite = okSprite;
        }

        public void SetDefaultSprites(Sprite aSprite, Sprite bSprite)
        {
            _defaultSprites[1] = aSprite;
            _defaultSprites[2] = bSprite;
        }

        public void DefaultSprite()
        {
            if (_lastIndex == -1) return;
            _editButtons[_lastIndex].sprite = _defaultSprites[_lastIndex];
            _lastIndex = -1;
        }
    }
}