using System;
using GameControl;
using GoogleMobileAds.Api;
using ManagerControl;
using UnityEngine;

public class AdmobManager : MonoBehaviour
{
    [SerializeField] private bool isTestMode;
    public event Action OnAdCloseEvent;
    private void Start()
    {
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize(null);
        LoadRewardedAd();
    }

    private void OnDisable()
    {
        OnAdCloseEvent = null;
    }

#region Rewarded

    private RewardedAd _rewardedAd;
    private const string TestRewardedId = "ca-app-pub-3940256099942544/1712485313";

#if UNITY_IPHONE
    private const string RewardedId = "ca-app-pub-6682177183839018/8095151824";
#elif UNITY_ANDROID
        private static string RewardedId = "";
#endif
    public void LoadRewardedAd()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        RewardedAd.Load(isTestMode ? TestRewardedId : RewardedId, adRequest, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                print("Rewarded failed to load" + error);
                return;
            }

            print("Rewarded ad loaded !!!!!!!!!!");
            _rewardedAd = ad;
            RewardedAdEvents(_rewardedAd);
        });
    }

    public void ShowRewardedAd()
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show(_ => { print("Give reward to player~~~~~~~~~~~~~~"); });
        }
        else
        {
            print("Rewarded ad not ready");
        }
    }

    public void RewardedAdEvents(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) => { Debug.Log($"Rewarded ad paid {adValue.Value} {adValue.CurrencyCode}."); };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () => { Debug.Log("Rewarded ad recorded an impression."); };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () => { Debug.Log("Rewarded ad was clicked."); };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () => { Debug.Log("Rewarded ad full screen content opened."); };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            SoundManager.MuteBGM(false);
            OnAdCloseEvent?.Invoke();
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content with error : " + error);
        };
    }

#endregion
}