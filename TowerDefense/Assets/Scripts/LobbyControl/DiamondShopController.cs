using System;
using System.Collections.Generic;
using BackEnd;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Random = UnityEngine.Random;

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
            // DataManager.diamond = diamond;
            // diamondText.text = diamond.ToString();
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
            PurchaseFreeDiamond();
        }

        private void OpenGoldPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            CameraManager.isControlActive = false;
            blockImage.enabled = true;
            shopPanel.DOAnchorPosY(0, 0.5f).SetEase(Ease.OutBack);
        }

        private void ClosePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            CameraManager.isControlActive = true;
            blockImage.enabled = false;
            shopPanel.DOAnchorPosY(Screen.height, 0.5f).SetEase(Ease.InBack);
        }

        private void PurchaseFreeDiamond()
        {
            print("FreeFreeFreeFreeFreeFreeFreeFreeFreeFreeFreeFreeFreeFreeFree");
            var bro = Backend.GameData.GetMyData("FreeDiaTable", Backend.UserInDate);
            if (bro.IsSuccess())
            {
                print("successsuccesssuccesssuccesssuccesssuccesssuccesssuccesssuccess");
                Debug.Log(bro);
            }
            else
            {
                print("failfailfailfailfailfailfailfailfailfailfailfailfailfailfailfail");
                var param = new Param();
                param.Add("diamonds", 0);
            }

            var refreshBro = Backend.GameData.GetMyData("FreeDiaTable", Backend.UserInDate);
            print("refreshrefreshrefreshrefreshrefreshrefreshrefreshrefreshrefreshrefreshrefreshrefreshrefresh");
            Debug.Log(refreshBro);
            var tbc = Backend.TBC.GetTBC().GetReturnValuetoJSON();
            var amountTBC = int.Parse(tbc["amountTBC"].ToString());
            print($"tbc: {amountTBC}");
        }

        public void PurchaseDiamond()
        {
            // var bro = Backend.TBC.GetTBC();
            // var json = bro.GetReturnValuetoJSON();
            // var amountTbc = int.Parse(json["amountTBC"].ToString());
            diamondText.text = DataManager.diamond.ToString();
        }
    }
}