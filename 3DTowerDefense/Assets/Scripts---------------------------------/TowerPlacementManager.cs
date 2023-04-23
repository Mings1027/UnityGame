using BuildControl;
using GameControl;
using ToolTipControl;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlacementManager : Singleton<TowerPlacementManager>
{
    private Vector3 placePos;
    private Quaternion placeRot;
    private Turret curSelectedTurret;
    private int towerIndex;
    private int lastIndex;
    private EventSystem eventSystem;

    private bool isTower;
    private bool isSell;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private ToolTipSystem tooltip;
    [SerializeField] private GameObject towerPanel;
    [SerializeField] private GameObject okButton;
    [SerializeField] private MeshRenderer rangeIndicator;
    [SerializeField] private string[] towerNames;
    [SerializeField] private TowerLevelManager[] towerLevelManagers;

    private void Awake()
    {
        eventSystem = EventSystem.current;
    }

    public void OpenTowerPanel(Transform t)
    {
        isTower = false;
        placePos = t.position;
        placeRot = t.rotation;
        towerPanel.SetActive(true);
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
    }

    private void ActiveOkButton()
    {
        var towerInfo = towerLevelManagers[towerIndex].towerLevels[0];
        okButton.transform.position = eventSystem.currentSelectedGameObject.transform.position;
        tooltip.Show(okButton.transform.position, towerInfo.towerInfo, towerInfo.towerName);
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
    }

    private void TowerUpgrade()
    {
        
    }

    private void SellTower()
    {
        curSelectedTurret.gameObject.SetActive(false);
    }

    private void CloseTowerPanel()
    {
        towerPanel.SetActive(false);
    }

    private void OpenEditPanel(Turret t)
    {
        isTower = true;
        curSelectedTurret = t;
        Physics.Raycast(curSelectedTurret.transform.position, Vector3.down, out var hit, 5, groundLayer);
        rangeIndicator.transform.position = hit.point;
        rangeIndicator.enabled = true;
    }

    private void PlaceTower()
    {
        StackObjectPool.Get<Turret>(towerNames[towerIndex], placePos).onOpenEditPanelEvent += OpenEditPanel;
    }
}