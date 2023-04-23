using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacementTileController : MonoBehaviour
{
    private void Start()
    {
        var towerPlacementManager = TowerPlacementManager.Instance;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out TowerPlacementTile t))
            {
                t.onOpenTowerPanelEvent += towerPlacementManager.OpenTowerPanel;
            }
        }
    }
}
