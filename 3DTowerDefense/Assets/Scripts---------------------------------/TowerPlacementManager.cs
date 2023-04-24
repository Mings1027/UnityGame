using System;
using BuildControl;
using DG.Tweening;
using GameControl;
using ManagerControl;
using TMPro;
using ToolTipControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TowerPlacementManager : Singleton<TowerPlacementManager>, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    private Tweener towerSelectPanelTween;
    private Tweener towerEditPanelTween;
    private Camera cam;
    private Vector3 touchPoint;
    private Vector3 placePos;
    private Quaternion placeRot;
    private Turret curSelectedTurret;
    private TowerPlacementTile curSelectedPlaceTile;
    private int towerIndex;
    private int lastIndex;

    private bool isTowerPanel, isEditPanel;
    private bool isTower;
    private bool isSell;

    public EventSystem eventSystem;

    [SerializeField] private InputManager input;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private ToolTipSystem tooltip;
    [SerializeField] private Transform towerSelectPanel;

    [SerializeField] private GameObject towerEditPanel;
    [SerializeField] private Button changeTypeButton;
    [SerializeField] private Button sellButton;

    [SerializeField] private GameObject okButton;
    [SerializeField] private MeshRenderer rangeIndicator;
    [SerializeField] private Transform content;
    [SerializeField] private Button[] towerButtons;
    [SerializeField] private TurretData[] turretData;

    private void Awake()
    {
        cam = Camera.main;
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
        changeTypeButton.onClick.AddListener(TurretChangeType);
        sellButton.onClick.AddListener(SellTower);

        input.onClosePanelEvent += CloseUI;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
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

    public void OpenTowerPanel(TowerPlacementTile towerPlacementTile, Transform t)
    {
        curSelectedPlaceTile = towerPlacementTile;
        isTower = false;
        placePos = t.position;
        placeRot = t.rotation;
        if (isTowerPanel) return;
        // towerSelectPanelTween.Restart();
    }

    private void TowerSelectButton(int index)
    {
        if (lastIndex != index)
        {
            TowerSelect(index);
        }
        else
        {
            lastIndex = -1;
            OkButton();
        }
    }

    private void TowerSelect(int index)
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
            else
            {
                TowerUpgrade();
            }
        }
        else
        {
            PlaceTower();
        }

        CloseUI();
    }

    private void TowerUpgrade()
    {
    }

    private void SellTower()
    {
        curSelectedTurret.gameObject.SetActive(false);
    }

    private void OpenEditPanel(Turret t)
    {
        touchPoint = t.transform.position;
        isTower = true;
        curSelectedTurret = t;
        Physics.Raycast(curSelectedTurret.transform.position, Vector3.down, out var hit, 5, groundLayer);
        rangeIndicator.transform.position = hit.point;
        rangeIndicator.enabled = true;
        // towerEditPanelTween.Restart();
        changeTypeButton.interactable = curSelectedTurret.BaseCount != 1;
    }

    private void CloseUI()
    {
        lastIndex = -1;
        if (isTowerPanel)
        {
            isTowerPanel = false;
            towerSelectPanelTween.PlayBackwards();
        }

        if (isEditPanel)
        {
            isEditPanel = false;
            towerEditPanelTween.PlayBackwards();
        }

        if (tooltip.gameObject.activeSelf)
        {
            tooltip.Hide();
        }

        if (okButton.gameObject.activeSelf)
        {
            okButton.gameObject.SetActive(false);
        }
    }

    private void PlaceTower()
    {
        if (curSelectedPlaceTile.IsPlaced) return;
        StackObjectPool.Get<Turret>(turretData[towerIndex].name, placePos + new Vector3(0, 0.5f, 0))
            .onOpenEditPanelEvent += OpenEditPanel;
        curSelectedPlaceTile.IsPlaced = true;
    }

    private void TurretChangeType()
    {
        curSelectedTurret.ChangeTurretType(curSelectedTurret.type == 0 ? 1 : 0);
    }
}