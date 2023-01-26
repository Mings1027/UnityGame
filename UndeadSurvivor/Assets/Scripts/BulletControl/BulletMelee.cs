using DG.Tweening;
using GameControl;
using PlayerControl;
using UnityEngine;

namespace BulletControl
{
    public class BulletMelee : MonoBehaviour
    {
        [SerializeField] private int damage;

        private SpriteRenderer _atkSprite;
        private Sequence _atkEffectSequence;

        private void Awake()
        {
            _atkSprite = GetComponent<SpriteRenderer>();
            _atkEffectSequence = DOTween.Sequence().SetAutoKill(false)
                .OnStart(() => transform.localScale = Vector2.zero)
                .Append(transform.DOScale(new Vector3(3, 1), 1))
                .Join(_atkSprite.DOFade(0, 1));
        }

        private void OnEnable()
        {
            _atkEffectSequence.Restart();
        }

        private void OnDestroy()
        {
            _atkEffectSequence.Kill();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            col.GetComponent<Health>().GetHit(damage, PlayerController.Rigid.gameObject);
        }
    }
}