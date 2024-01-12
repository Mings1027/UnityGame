using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class GoogleAdmobManager : MonoBehaviour
{
    private string addID = "ca-app-pub-6682177183839018~6910116563";
    private string rewardedID = "ca-app-pub-3940256099942544/1712485313";

    private RewardedAd _rewardedAd;

    private void Start()
    {
        LoadRewardedAd();
    }

#region Rewarded

    public void LoadRewardedAd()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        RewardedAd.Load(rewardedID, adRequest, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                print("Rewarded failed to Load" + error);
                return;
            }

            print("Rewarded ad Loaded !!!!!!");
            _rewardedAd = ad;
            RegisterEventHandlers(_rewardedAd);
        });
    }

    public void ShowRewardedAd()
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((reward) =>
            {
                print("Give reward to player !!!!");
                //Get Golds!
            });
        }
        else print("Rewarded ad not ready");
    }

    private void RegisterEventHandlers(RewardedAd ad)
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
        ad.OnAdFullScreenContentClosed += () => { Debug.Log("Rewarded ad full screen content closed."); };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

#endregion
}