using DG.Tweening;
using GameControl;
using UIControl;
using UnityEngine;
using UnityEngine.UI;

namespace BuildControl
{
    public class BuildingPointController : MonoBehaviour
    {
        private Sequence buildingPointSequence;

        [SerializeField] private Button startGameButton;

        private void Start()
        {
            var uiManager = UIManager.Instance;

            buildingPointSequence = DOTween.Sequence().SetAutoKill(false);
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var p = StackObjectPool.Get<BuildingPoint>("BuildingPoint", child.position + new Vector3(0, 300, 0),
                    child.rotation);
                p.onOpenTowerSelectPanelEvent += uiManager.OpenTowerSelectPanel;
                buildingPointSequence.Append(p.transform.DOMoveY(0, 0.2f)
                    .OnComplete(() => StackObjectPool.Get("BuildSmoke", p.transform.position)));
            }

            buildingPointSequence.Pause();

            startGameButton.onClick.AddListener(StartGameButton);
        }

        private void StartGameButton()
        {
            buildingPointSequence.Restart();
            startGameButton.gameObject.SetActive(false);
        }
    }
}