using System;
using DataControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerButtonController : MonoBehaviour
    {
        private int _lastIndex;
        [SerializeField] private Sprite okSprite;

        private Button[] _towerButtons;
        private Sprite[] _defaultSprites;

        private void Awake()
        {
            _lastIndex = -1;
            _towerButtons = new Button[transform.childCount];
            _defaultSprites = new Sprite[_towerButtons.Length];

            for (var i = 0; i < _towerButtons.Length; i++)
            {
                _towerButtons[i] = transform.GetChild(i).GetChild(0).GetComponent<Button>();
                var index = i;
                _towerButtons[i].onClick.AddListener(() =>
                {
                    CheckButton(index);
                    SoundManager.Instance.PlaySound("ButtonSound");
                });
                _defaultSprites[i] = _towerButtons[i].image.sprite;
            }

            gameObject.SetActive(false);
        }

        private void CheckButton(int index)
        {
            if (_lastIndex != -1)
            {
                _towerButtons[_lastIndex].image.sprite = _defaultSprites[_lastIndex];
            }

            _lastIndex = index;

            _towerButtons[index].image.sprite = okSprite;
        }

        public void DefaultSprite()
        {
            if (_lastIndex == -1) return;
            _towerButtons[_lastIndex].image.sprite = _defaultSprites[_lastIndex];
            _lastIndex = -1;
        }
    }
}