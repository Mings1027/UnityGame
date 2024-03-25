using AdsControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace LobbyUIControl
{
    public class DiamondShopController : MonoBehaviour
    {
        private AdmobManager _rewardedAds;
        private Sequence _panelSequence;

        [SerializeField] private Button freePurchaseButton;
        [SerializeField] private Transform loadingImage;

        private void Start()
        {
            _rewardedAds = FindAnyObjectByType<AdmobManager>();
            loadingImage.localScale = Vector3.zero;

            freePurchaseButton.onClick.AddListener(() =>
            {
                freePurchaseButton.interactable = false;
                ShowRewardedAd().Forget();
            });
        }

        private void OnDisable()
        {
            _panelSequence?.Kill();
        }

        private async UniTaskVoid ShowRewardedAd()
        {
            SoundManager.MuteBGM(true);
            loadingImage.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetUpdate(true);
            loadingImage.DOLocalRotate(new Vector3(0, 0, -360), 1, RotateMode.FastBeyond360).SetLoops(2);
            await UniTask.Delay(Random.Range(5, 8) * 100);
            freePurchaseButton.interactable = true;
            loadingImage.localScale = Vector3.zero;
            _rewardedAds.ShowRewardedAd();
        }
    }
}