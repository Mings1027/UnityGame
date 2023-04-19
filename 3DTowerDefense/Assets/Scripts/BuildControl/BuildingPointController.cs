using DG.Tweening;
using GameControl;
using UIControl;
using UnityEngine;

namespace BuildControl
{
    public class BuildingPointController : Singleton<BuildingPointController>
    {
        public Sequence BuildPointSequence { get; private set; }

        private void Start()
        {
            var uiManager = UIManager.Instance;

            BuildPointSequence = DOTween.Sequence().SetAutoKill(false);
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var p = StackObjectPool.Get<BuildingPoint>("BuildingPoint", child.position + new Vector3(0, 300, 0),
                    child.rotation);
                p.onOpenTowerSelectPanelEvent += uiManager.OpenTowerSelectPanel;
                BuildPointSequence.Append(p.transform.DOMoveY(0, 0.2f)
                    .OnComplete(() => StackObjectPool.Get("BuildSmoke", p.transform.position)));
            }

            BuildPointSequence.Pause();

        }

    }
}