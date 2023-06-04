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

        [SerializeField] private Button[] towerButtons;
        [SerializeField] private Sprite[] defaultSprites;

        private void Awake()
        {
            _lastIndex = -1;
            towerButtons = new Button[transform.childCount];
            defaultSprites = new Sprite[towerButtons.Length];

            for (var i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i] = transform.GetChild(i).GetChild(0).GetComponent<Button>();
                var index = i;
                towerButtons[i].onClick.AddListener(() =>
                {
                    CheckButton(index);
                    SoundManager.Instance.PlaySound("ButtonClick");
                });
                defaultSprites[i] = towerButtons[i].image.sprite;
            }
            gameObject.SetActive(false);
        }

        public void DefaultSprite()
        {
            if (_lastIndex == -1) return;
            towerButtons[_lastIndex].image.sprite = defaultSprites[_lastIndex];
            _lastIndex = -1;
        }

        private void CheckButton(int index)
        {
            if (_lastIndex != -1)
            {
                towerButtons[_lastIndex].image.sprite = defaultSprites[_lastIndex];
            }

            _lastIndex = index;

            towerButtons[index].image.sprite = okSprite;
        }
    }
}