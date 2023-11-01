using System;
using DataControl;
using TestControl;
using TMPro;
using TowerControl;
using UnityEngine;

public enum Path
{
    TowerName,
    TowerInfo,
    NumberText
}

public class TestController : MonoBehaviour
{
    private TMP_Text _tmpText;
    [SerializeField] private Path path;

    private void Awake()
    {
        _tmpText = GetComponent<TMP_Text>();
    }

    private void UpdateText()
    {
        if (path == Path.TowerName)
        {
            
        }
    }
}