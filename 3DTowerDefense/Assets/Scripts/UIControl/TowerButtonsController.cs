using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerButtonsController : MonoBehaviour
    {
        private int _lastIndex;
        private Image[] _towerBtnImages;
        private Image[] _towerEditBtnImages;
        private Sprite[] _towerBtnDefaultSprites;
        private Sprite[] _towerEditBtnDefaultSprites;

        [SerializeField] private Sprite okSprite;
        [SerializeField] private Transform towerButtons;
        [SerializeField] private Transform towerEditButtons;

        private void Awake()
        {
            _lastIndex = -1;
            _towerBtnImages = new Image[towerButtons.childCount];
            _towerEditBtnImages = new Image[towerEditButtons.childCount];

            _towerBtnDefaultSprites = new Sprite[_towerBtnImages.Length];
            _towerEditBtnDefaultSprites = new Sprite[_towerEditBtnImages.Length];

            for (int i = 0; i < _towerBtnImages.Length; i++)
            {
                var index = i;
                towerButtons.GetChild(i).GetComponent<Button>().onClick.AddListener(() => ChangeBtnSprite(index));
                _towerBtnImages[i] = towerButtons.GetChild(i).GetChild(0).GetComponent<Image>();
                _towerBtnDefaultSprites[i] = _towerBtnImages[i].sprite;
            }

            for (int i = 0; i < _towerEditBtnImages.Length; i++)
            {
                var index = i;
                towerEditButtons.GetChild(i).GetComponent<Button>().onClick
                    .AddListener(() => ChangeBtnSprite(index, false));
                _towerEditBtnImages[i] = towerEditButtons.GetChild(i).GetChild(0).GetComponent<Image>();
                _towerEditBtnDefaultSprites[i] = _towerEditBtnImages[i].sprite;
            }
        }

        private void ChangeBtnSprite(int index, bool isTowerBtn = true)
        {
            if (_lastIndex != -1)
            {
                if (isTowerBtn)
                {
                    _towerBtnImages[_lastIndex].sprite = _towerBtnDefaultSprites[_lastIndex];
                }
                else
                {
                    _towerEditBtnImages[_lastIndex].sprite = _towerEditBtnDefaultSprites[_lastIndex];
                }
            }

            if (isTowerBtn)
            {
                _towerBtnImages[index].sprite = okSprite;
            }
            else
            {
                _towerEditBtnImages[index].sprite = okSprite;
            }

            _lastIndex = index;
        }

        public void SetDefaultSprite()
        {
            if (_lastIndex == 1) return;
            _towerBtnImages[_lastIndex].sprite = _towerBtnDefaultSprites[_lastIndex];
            _towerEditBtnImages[_lastIndex].sprite = _towerEditBtnDefaultSprites[_lastIndex];
            _lastIndex = -1;
        }

        public void SetUpgradeBtnSprite(Sprite aSprite, Sprite bSprite)
        {
            _towerEditBtnDefaultSprites[1] = aSprite;
            _towerEditBtnDefaultSprites[2] = bSprite;
        }
    }
}