using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GoogleMobileAdmob;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class RewardPanelController : MonoBehaviour
    {
        [SerializeField] private Button goldButton;
        [SerializeField] private Button freeRewardAdButton;
        [SerializeField] private Transform loadingImage;

        private void Awake()
        {
            goldButton.onClick.AddListener(OpenGoldPanel);
            loadingImage.localScale = Vector3.zero;
        }

        private void Start()
        {
            transform.localScale = Vector3.zero;
            transform.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(ClosePanel);
            freeRewardAdButton.onClick.AddListener(() => ShowRewardedAd().Forget());
        }

        private async UniTaskVoid ShowRewardedAd()
        {
            loadingImage.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetUpdate(true);
            var ranTime = Random.Range(0.8f, 1.5f);
            loadingImage.DOLocalRotate(new Vector3(0, 0, -360), 1, RotateMode.FastBeyond360).SetLoops(2);
            await UniTask.Delay((int)(ranTime * 1000));
            AdmobManager.Instance.ShowRewardedAd();
            SoundManager.Instance.MuteBGM(true);
            loadingImage.localScale = Vector3.zero;
        }

        private void OpenGoldPanel()
        {
            SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
            Input.multiTouchEnabled = false;
            CameraManager.isControlActive = false;
            transform.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetUpdate(true);
        }

        private void ClosePanel()
        {
            SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
            Input.multiTouchEnabled = true;
            CameraManager.isControlActive = true;
            transform.DOScale(0, 0.25f).From(1).SetEase(Ease.InBack).SetUpdate(true);
        }
    }
}