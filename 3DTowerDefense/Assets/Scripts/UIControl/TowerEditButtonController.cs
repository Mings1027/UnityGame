using System;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerEditButtonController : MonoBehaviour
    {
        private int _lastIndex;
        [SerializeField] private Sprite okSprite;
        [SerializeField] private Button[] editButtons;
        [SerializeField] private Sprite[] defaultSprites;

        private void Awake()
        {
            editButtons = new Button[transform.childCount];
            defaultSprites = new Sprite[editButtons.Length];
            for (int i = 0; i < editButtons.Length; i++)
            {
                editButtons[i] = transform.GetChild(i).GetChild(0).GetComponent<Button>();
                editButtons[i].onClick.AddListener(() => { SoundManager.Instance.PlaySound("ButtonClick"); });
                defaultSprites[i] = editButtons[i].image.sprite;
            }

            gameObject.SetActive(false);
        }

        public void DefaultSprite()
        {
            if (_lastIndex == -1) return;
            editButtons[_lastIndex].image.sprite = defaultSprites[_lastIndex];
            _lastIndex = -1;
        }

        public bool ClickButton(int index)
        {
            if (_lastIndex != -1)
            {
                editButtons[_lastIndex].image.sprite = defaultSprites[_lastIndex];
                return true;
            }

            _lastIndex = index;
            editButtons[index].image.sprite = okSprite;
            return false;
        }
    }
}