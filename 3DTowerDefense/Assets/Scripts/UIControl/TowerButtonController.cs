using System;
using BuildControl;
using DG.Tweening;
using GameControl;
using ManagerControl;
using TowerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerButtonController : MonoBehaviour
    {
        private int _lastIndex;
        private TowerButton[] _towerButtons;
        private string[] towerNames;
        private UIManager _uiManager;

        public Sequence TowerSelectPanelSequence { get; private set; }
        public Transform BuildTransform { get; set; }

        [SerializeField] private MeshFilter curTowerMesh;
        [SerializeField] private MeshRenderer towerRangeIndicator;
        [SerializeField] private TowerLevelManager[] towerLevelManagers;

        private void Awake()
        {
            _lastIndex = -1;
            _uiManager = UIManager.Instance;

            _towerButtons = new TowerButton[transform.childCount];
            TowerSelectPanelSequence = DOTween.Sequence().SetAutoKill(false).Pause();

            for (int i = 0; i < _towerButtons.Length; i++)
            {
                _towerButtons[i] = transform.GetChild(i).GetComponent<TowerButton>();
                var index = i;
                _towerButtons[i].GetComponent<Button>().onClick
                    .AddListener(() => SpawnTower(index));
                TowerSelectPanelSequence.Append(_towerButtons[i].transform.DOScale(1, 0.05f).From(0))
                    .Join(_towerButtons[i].transform.DOLocalMove(Vector3.zero, 0.05f).From());
            }

            towerNames = new string[transform.childCount];
            for (int i = 0; i < towerNames.Length; i++)
            {
                towerNames[i] = transform.GetChild(i).name
                    .Replace("Button", "");
            }
        }

        private void SpawnTower(int index)
        {
            var tempTowerLevel = towerLevelManagers[index].towerLevels[0];
            curTowerMesh.transform.SetPositionAndRotation(BuildTransform.position, BuildTransform.rotation);
            curTowerMesh.sharedMesh = tempTowerLevel.towerMesh.sharedMesh;

            if (_lastIndex == index)
            {
                var t = StackObjectPool.Get<Tower>(towerNames[index], BuildTransform);
                t.onOpenTowerEditPanelEvent += _uiManager.OpenTowerEditPanel;
                BuildTransform.gameObject.SetActive(false);
                _uiManager.TowerUpgrade().Forget();
                _uiManager.CloseUI();
                _lastIndex = -1;
            }
            else
            {
                _lastIndex = index;

                //ok버튼 나오게 하고 
            }

            var indicatorTransform = towerRangeIndicator.transform;
            indicatorTransform.position = curTowerMesh.transform.position;
            indicatorTransform.localScale =
                new Vector3(tempTowerLevel.attackRange * 2, 0.1f, tempTowerLevel.attackRange * 2);

            if (towerRangeIndicator.enabled) return;
            towerRangeIndicator.enabled = true;
        }
    }
}