using DG.Tweening;
using UnityEngine;

namespace PerlinNoiseControl
{
    public class Crystal : MonoBehaviour
    {
        private Sequence _generateCrystalSequence;
       [SerializeField] private Ease crystalEase;

        private void Awake()
        {
            _generateCrystalSequence = DOTween.Sequence()
                .SetAutoKill(false)
                .Append(transform.DOLocalMoveY(0, 2.5f)
                    .From(100)
                    .SetEase(crystalEase));
        }

        private void OnEnable()
        {
            _generateCrystalSequence.Restart();
        }
    }
}