using GameControl;
using TowerControl;
using UnityEngine;

namespace BuildControl
{
    [DisallowMultipleComponent]
    public class BuildController : Singleton<BuildController>
    {
        public Tower SelectedTower { get; set; }
    }
}