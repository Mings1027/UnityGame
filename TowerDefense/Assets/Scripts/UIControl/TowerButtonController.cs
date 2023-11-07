using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DataControl;
using ManagerControl;
using UnityEngine;

namespace UIControl
{
    public class TowerButtonController : MonoBehaviour
    {
        private InputManager _inputManager;
        private Dictionary<int, TowerData> _towerButtonDic;

        public void Init()
        {
            _inputManager = FindObjectOfType<InputManager>();
            _towerButtonDic = new Dictionary<int, TowerData>();
            var towerButtons = transform.GetChild(0);
            for (var i = 0; i < towerButtons.childCount; i++)
            {
                var towerButton = towerButtons.GetChild(i).GetComponent<TowerButton>();
                towerButton.buttonIndex = (byte)i;
                towerButton.OnClick += ClickTowerButton;
            }
        }

        private void ClickTowerButton(int index, Vector3 pos)
        {
            _inputManager.enabled = true;
            _inputManager.StartPlacement(_towerButtonDic[index].TowerType, _towerButtonDic[index].IsUnitTower);
            OpenTowerInfo(_towerButtonDic[index], pos).Forget();
        }

        private async UniTaskVoid OpenTowerInfo(TowerData towerData, Vector3 pos)
        {
            await UniTask.Delay(300);
            if (UIManager.Instance.IsOnUI)
            {
                UIManager.Instance.towerCardUI.OpenTowerCard(pos, towerData);
            }
        }

        public void SetDictionary(int index, TowerData towerData) => _towerButtonDic.Add(index, towerData);
    }
}