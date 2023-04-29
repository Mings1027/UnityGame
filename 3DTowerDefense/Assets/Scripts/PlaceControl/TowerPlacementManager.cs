using DG.Tweening;
using GameControl;
using TMPro;
using ToolTipControl;
using TurretControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlaceControl
{
    public class TowerPlacementManager : Singleton<TowerPlacementManager>, IPointerDownHandler, IPointerUpHandler
    {
        private Tweener _towerSelectPanelTween;
        private Tweener _towerEditPanelTween;
        private Vector3 _placePos;
        private Quaternion _placeRot;
        private Turret _curSelectedTurret;
        private TowerPlacementTile _curSelectedPlaceTile;
        private int _towerIndex;
        private int _lastIndex;

        private bool _isTowerPanel, _isEditPanel;
        private bool _isTower;
        private bool _isSell;

        private EventSystem _eventSystem;

        [SerializeField] private TouchMap touchMap;

        [SerializeField] private ToolTipSystem tooltip;
        [SerializeField] private Transform towerSelectPanel;

        [SerializeField] private GameObject towerEditPanel;
        [SerializeField] private Button turretUpgradeButton;
        [SerializeField] private Button sellButton;

        [SerializeField] private GameObject okButton;
        [SerializeField] private Transform content;
        [SerializeField] private Button[] towerButtons;
        [SerializeField] private TurretData[] turretData;

        private void Awake()
        {
            _lastIndex = -1;
            _eventSystem = EventSystem.current;
            Input.multiTouchEnabled = false;

            _towerSelectPanelTween = towerSelectPanel.transform.DOLocalMoveX(-450, 0.5f).SetRelative()
                .SetAutoKill(false).Pause().OnComplete(() => _isTowerPanel = true);
            _towerEditPanelTween = towerEditPanel.transform.DOLocalMoveY(300, 0.5f).SetRelative()
                .SetAutoKill(false).Pause().OnComplete(() => _isEditPanel = true);

            towerButtons = new Button[content.childCount];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i] = content.GetChild(i).GetComponent<Button>();
                towerButtons[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                    turretData[i].turretInfos[0].turretName;
                var index = i;
                towerButtons[i].onClick.AddListener(() => TowerButton(index));
            }

            okButton.GetComponent<Button>().onClick.AddListener(OkButton);
            turretUpgradeButton.onClick.AddListener(TurretUpgrade);
            sellButton.onClick.AddListener(SellTower);

            touchMap.onClosePanelEvent += CloseUI;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        private void Update()
        {
            if (okButton.gameObject.activeSelf)
            {
                okButton.transform.position = _eventSystem.currentSelectedGameObject.transform.position;
            }
        }

        public void OpenTowerSelectPanel(TowerPlacementTile tile, Transform t)
        {
            if (tile == _curSelectedPlaceTile) return;
            ResetOutline();
            _curSelectedPlaceTile = tile;
            _isTower = false;
            _placePos = t.position;
            _placeRot = t.rotation;
            if (_isTowerPanel) return;
            _towerSelectPanelTween.Restart();
        }

        private void OpenEditPanel(Turret t)
        {
            if (t == _curSelectedTurret) return;
            ResetOutline();
            turretUpgradeButton.interactable = !t.IsUpgraded;
            _isTower = true;

            _curSelectedTurret = t;
            if (_isEditPanel) return;
            _towerEditPanelTween.Restart();
        }

        private void ResetOutline()
        {
            if (_curSelectedTurret != null)
            {
                _curSelectedTurret.Outline.enabled = false;
                _curSelectedTurret = null;
            }

            if (_curSelectedPlaceTile != null)
            {
                _curSelectedPlaceTile.Outline.enabled = false;
                _curSelectedPlaceTile = null;
            }

            _isTowerPanel = false;
            _towerSelectPanelTween.PlayBackwards();
        
            _isEditPanel = false;
            _towerEditPanelTween.PlayBackwards();
        }

        private void CloseUI()
        {
            ResetOutline();
            _lastIndex = -1;

            if (tooltip.gameObject.activeSelf)
            {
                tooltip.Hide();
            }

            if (okButton.gameObject.activeSelf)
            {
                okButton.gameObject.SetActive(false);
            }

            _curSelectedTurret = null;
            _curSelectedPlaceTile = null;
        }

        private void TowerButton(int index)
        {
            if (_lastIndex != index)
            {
                SelectedOtherButton(index);
            }
            else
            {
                _lastIndex = -1;
                OkButton();
            }
        }

        private void SelectedOtherButton(int index)
        {
            _towerIndex = index;
            _lastIndex = index;
            ActiveOkButton();

            var aboutTurret = turretData[_towerIndex].turretInfos[0];
            tooltip.Show(okButton.transform.position, aboutTurret.turretDesc, aboutTurret.turretName);
        }

        private void ActiveOkButton()
        {
            okButton.transform.position = _eventSystem.currentSelectedGameObject.transform.position;
            okButton.SetActive(true);
        }

        private void OkButton()
        {
            if (_isTower)
            {
                if (_isSell)
                {
                    SellTower();
                }
            }
            else
            {
                PlaceTower();
            }

            CloseUI();
        }

        private void SellTower()
        {
            _curSelectedTurret.gameObject.SetActive(false);
            _curSelectedTurret = null;
        }
    
        private void PlaceTower()
        {
            var t = StackObjectPool.Get<Turret>(turretData[_towerIndex].name, _placePos + new Vector3(0, 0.5f, 0));
            t.onOpenEditPanelEvent += OpenEditPanel;
        }

        private void TurretUpgrade()
        {
            if (_curSelectedTurret == null) return;
            if (_curSelectedTurret.IsUpgraded) return;
            _curSelectedTurret.gameObject.SetActive(false);
            _curSelectedTurret =
                StackObjectPool.Get<Turret>(_curSelectedTurret.name + "2", _curSelectedTurret.transform.position);
            _curSelectedTurret.onOpenEditPanelEvent += OpenEditPanel;
            _curSelectedTurret.IsUpgraded = true;
            // curSelectedTurret.Upgrade();

            turretUpgradeButton.interactable = false;

            CloseUI();
        }
    }
}