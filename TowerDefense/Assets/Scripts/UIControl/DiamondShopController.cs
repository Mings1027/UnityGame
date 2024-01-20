using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UIControl
{
    public class DiamondShopController : MonoBehaviour
    {
        private AdmobManager _admobManager;
        [SerializeField] private TMP_Text diamondText;
        [SerializeField] private RectTransform shopPanel;
        [SerializeField] private Image blockImage;
        [SerializeField] private Button diamondButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button freePurchaseButton;
        [SerializeField] private Transform loadingImage;

        private void Awake()
        {
            _admobManager = FindAnyObjectByType<AdmobManager>();
            diamondButton.onClick.AddListener(OpenGoldPanel);
            closeButton.onClick.AddListener(ClosePanel);
            loadingImage.localScale = Vector3.zero;
            var diamond = PlayerPrefs.GetInt(StringManager.Diamond);
            DataManager.diamond = diamond;
            diamondText.text = diamond.ToString();
        }

        private void Start()
        {
            shopPanel.anchoredPosition = new Vector2(0, Screen.height);
            blockImage.enabled = false;
            freePurchaseButton.onClick.AddListener(() =>
            {
                freePurchaseButton.interactable = false;
                ShowRewardedAd().Forget();
            });
            SoundManager.PlayBGM(SoundEnum.GameStart);
            _admobManager.OnAdCloseEvent += () => { freePurchaseButton.interactable = true; };
        }

        private async UniTaskVoid ShowRewardedAd()
        {
            SoundManager.MuteBGM(true);
            loadingImage.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetUpdate(true);
            loadingImage.DOLocalRotate(new Vector3(0, 0, -360), 1, RotateMode.FastBeyond360).SetLoops(2);
            await UniTask.Delay(Random.Range(5, 8) * 100);
            loadingImage.localScale = Vector3.zero;
            _admobManager.ShowRewardedAd();
            PurchaseDiamond(100);
        }

        private void OpenGoldPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            Input.multiTouchEnabled = false;
            CameraManager.isControlActive = false;
            blockImage.enabled = true;
            shopPanel.DOAnchorPosY(0, 0.5f).SetEase(Ease.OutBack);
        }

        private void ClosePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            Input.multiTouchEnabled = true;
            CameraManager.isControlActive = true;
            blockImage.enabled = false;
            shopPanel.DOAnchorPosY(Screen.height, 0.5f).SetEase(Ease.InBack);
        }

        public void PurchaseDiamond(int diamond)
        {
            DataManager.diamond += diamond;
            PlayerPrefs.SetInt(StringManager.Diamond, DataManager.diamond);
            diamondText.text = DataManager.diamond.ToString();
        }
    }
}