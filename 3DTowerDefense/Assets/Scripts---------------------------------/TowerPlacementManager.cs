using DG.Tweening;
using GameControl;
using TMPro;
using ToolTipControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerPlacementManager : Singleton<TowerPlacementManager>, IPointerDownHandler, IPointerUpHandler
{
    private Tweener towerSelectPanelTween;
    private Tweener towerEditPanelTween;
    private Vector3 placePos;
    private Quaternion placeRot;
    private Turret curSelectedTurret;
    private TowerPlacementTile curSelectedPlaceTile;
    private int towerIndex;
    private int lastIndex;

    private bool isTowerPanel, isEditPanel;
    private bool isTower;
    private bool isSell;

    private EventSystem eventSystem;

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
        lastIndex = -1;
        eventSystem = EventSystem.current;
        Input.multiTouchEnabled = false;

        towerSelectPanelTween = towerSelectPanel.transform.DOLocalMoveX(-450, 0.5f).SetRelative()
            .SetAutoKill(false).Pause().OnComplete(() => isTowerPanel = true);
        towerEditPanelTween = towerEditPanel.transform.DOLocalMoveY(300, 0.5f).SetRelative()
            .SetAutoKill(false).Pause().OnComplete(() => isEditPanel = true);

        towerButtons = new Button[content.childCount];
        for (var i = 0; i < towerButtons.Length; i++)
        {
            towerButtons[i] = content.GetChild(i).GetComponent<Button>();
            towerButtons[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                turretData[i].turretInfos[0].turretName;
            var index = i;
            towerButtons[i].onClick.AddListener(() => TowerSelectButton(index));
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
            okButton.transform.position = eventSystem.currentSelectedGameObject.transform.position;
        }
    }

    public void OpenTowerPanel(TowerPlacementTile tile, Transform t)
    {
        if (tile == curSelectedPlaceTile) return;
        ResetOutline();
        curSelectedPlaceTile = tile;
        isTower = false;
        placePos = t.position;
        placeRot = t.rotation;
        if (isTowerPanel) return;
        towerSelectPanelTween.Restart();
    }

    private void OpenEditPanel(Turret t)
    {
        if (t == curSelectedTurret) return;
        ResetOutline();
        turretUpgradeButton.interactable = !t.IsUpgraded;
        isTower = true;

        curSelectedTurret = t;
        if (isEditPanel) return;
        towerEditPanelTween.Restart();
    }

    private void ResetOutline()
    {
        if (curSelectedTurret != null)
        {
            curSelectedTurret.Outline.enabled = false;
        }

        if (curSelectedPlaceTile != null)
        {
            curSelectedPlaceTile.Outline.enabled = false;
        }

        isTowerPanel = false;
        towerSelectPanelTween.PlayBackwards();
        
        isEditPanel = false;
        towerEditPanelTween.PlayBackwards();
    }

    private void CloseUI()
    {
        ResetOutline();
        lastIndex = -1;

        if (tooltip.gameObject.activeSelf)
        {
            tooltip.Hide();
        }

        if (okButton.gameObject.activeSelf)
        {
            okButton.gameObject.SetActive(false);
        }

        curSelectedTurret = null;
        curSelectedPlaceTile = null;
    }

    private void TowerSelectButton(int index)
    {
        if (lastIndex != index)
        {
            SelectedOtherButton(index);
        }
        else
        {
            lastIndex = -1;
            OkButton();
        }
    }

    private void SelectedOtherButton(int index)
    {
        towerIndex = index;
        lastIndex = index;
        ActiveOkButton();

        var aboutTurret = turretData[towerIndex].turretInfos[0];
        tooltip.Show(okButton.transform.position, aboutTurret.turretDesc, aboutTurret.turretName);
    }

    private void ActiveOkButton()
    {
        okButton.transform.position = eventSystem.currentSelectedGameObject.transform.position;
        okButton.SetActive(true);
    }

    private void OkButton()
    {
        if (isTower)
        {
            if (isSell)
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
        curSelectedTurret.gameObject.SetActive(false);
        curSelectedTurret = null;
    }
    
    private void PlaceTower()
    {
        var t = StackObjectPool.Get<Turret>(turretData[towerIndex].name, placePos + new Vector3(0, 0.5f, 0));
        t.onOpenEditPanelEvent += OpenEditPanel;
    }

    private void TurretUpgrade()
    {
        if (curSelectedTurret == null) return;
        if (curSelectedTurret.IsUpgraded) return;
        curSelectedTurret.gameObject.SetActive(false);
        curSelectedTurret =
            StackObjectPool.Get<Turret>(curSelectedTurret.name + "2", curSelectedTurret.transform.position);
        curSelectedTurret.onOpenEditPanelEvent += OpenEditPanel;
        curSelectedTurret.IsUpgraded = true;
        curSelectedTurret.Upgrade();

        turretUpgradeButton.interactable = false;

        CloseUI();
    }
}