using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;
using System;

namespace Game.Ads
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;
        private RewardedInterstitialAd _rewardedInterstitialAd;

        [Header("AdMob configuration (Android Test IDs by default)")]
        [Tooltip("For reference only. Note: In AdMob, you must also set the App ID globally in Unity via: Assets -> Google Mobile Ads -> Settings")]
        [SerializeField] private string _adMobAppId = "ca-app-pub-3940256099942544~3347511713";
        [SerializeField] private string _bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
        [SerializeField] private string _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
        [SerializeField] private string _rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
        [SerializeField] private string _rewardedInterstitialAdUnitId = "ca-app-pub-3940256099942544/5354046379";

        private Action _onRewardedCallback;
        private bool _isAdMobEnabled = false;

        public bool IsNetworkConnected()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!IsNetworkConnected())
            {
                Debug.LogWarning("[AdManager] No internet connection on startup. AdMob disabled.");
                _isAdMobEnabled = false;
                return;
            }

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                Debug.Log("[AdManager] Google Mobile Ads SDK Initialized.");
                _isAdMobEnabled = true;
                
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    LoadBannerAd();
                    LoadInterstitialAd();
                    LoadRewardedAd();
                    LoadRewardedInterstitialAd();
                });
            });
        }

        #region Banner Ad
        public void LoadBannerAd()
        {
            if (!_isAdMobEnabled || !IsNetworkConnected()) return;

            if (_bannerView != null)
            {
                _bannerView.Destroy();
            }

            _bannerView = new BannerView(_bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
            AdRequest adRequest = new AdRequest();
            _bannerView.LoadAd(adRequest);
            Debug.Log("[AdManager] Banner Ad Loading...");
        }

        public void ShowBanner(bool show)
        {
            if (!_isAdMobEnabled || !IsNetworkConnected()) return;

            if (_bannerView != null)
            {
                if (show)
                    _bannerView.Show();
                else
                    _bannerView.Hide();
            }
        }
        #endregion

        #region Interstitial Ad
        public void LoadInterstitialAd()
        {
            if (!_isAdMobEnabled || !IsNetworkConnected()) return;

            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            AdRequest adRequest = new AdRequest();

            InterstitialAd.Load(_interstitialAdUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError("[AdManager] Interstitial ad failed to load: " + error);
                        return;
                    }

                    Debug.Log("[AdManager] Interstitial ad loaded.");
                    _interstitialAd = ad;

                    _interstitialAd.OnAdFullScreenContentClosed += () =>
                    {
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            Debug.Log("[AdManager] Interstitial ad closed. Reloading...");
                            LoadInterstitialAd();
                        });
                    };
                });
        }

        public void ShowInterstitial()
        {
            if (!_isAdMobEnabled || !IsNetworkConnected())
            {
                Debug.LogWarning("[AdManager] AdMob disabled or no internet. Skipping Interstitial.");
                return;
            }

            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _interstitialAd.Show();
            }
            else
            {
                Debug.LogWarning("[AdManager] Interstitial ad not ready. Reloading...");
                LoadInterstitialAd();
            }
        }
        #endregion

        #region Rewarded Ad
        public void LoadRewardedAd()
        {
            if (!_isAdMobEnabled || !IsNetworkConnected()) return;

            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            AdRequest adRequest = new AdRequest();

            RewardedAd.Load(_rewardedAdUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError("[AdManager] Rewarded ad failed to load: " + error);
                        return;
                    }

                    Debug.Log("[AdManager] Rewarded ad loaded.");
                    _rewardedAd = ad;

                    _rewardedAd.OnAdFullScreenContentClosed += () =>
                    {
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            Debug.Log("[AdManager] Rewarded ad closed. Reloading...");
                            LoadRewardedAd();
                        });
                    };
                });
        }

        public void ShowRewarded(Action onRewardedComplete)
        {
            if (!_isAdMobEnabled || !IsNetworkConnected())
            {
                Debug.LogWarning("[AdManager] AdMob disabled or no internet. Bypassing ad to prevent lockup.");
                onRewardedComplete?.Invoke();
                return;
            }

            _onRewardedCallback = onRewardedComplete;

            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAd.Show((Reward reward) =>
                {
                    MobileAdsEventExecutor.ExecuteInUpdate(() =>
                    {
                        Debug.Log("[AdManager] User earned reward: " + reward.Amount + " " + reward.Type);
                        if (_onRewardedCallback != null)
                        {
                            _onRewardedCallback.Invoke();
                            _onRewardedCallback = null;
                        }
                    });
                });
            }
            else
            {
                Debug.LogWarning("[AdManager] Rewarded ad not ready. Bypassing ad as fallback to prevent lockup.");
                onRewardedComplete?.Invoke();
                LoadRewardedAd();
            }
        }
        #endregion

        #region Rewarded Interstitial Ad
        public void LoadRewardedInterstitialAd()
        {
            if (!_isAdMobEnabled || !IsNetworkConnected()) return;

            if (_rewardedInterstitialAd != null)
            {
                _rewardedInterstitialAd.Destroy();
                _rewardedInterstitialAd = null;
            }

            AdRequest adRequest = new AdRequest();

            RewardedInterstitialAd.Load(_rewardedInterstitialAdUnitId, adRequest,
                (RewardedInterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError("[AdManager] Rewarded Interstitial ad failed to load: " + error);
                        return;
                    }

                    Debug.Log("[AdManager] Rewarded Interstitial ad loaded.");
                    _rewardedInterstitialAd = ad;

                    _rewardedInterstitialAd.OnAdFullScreenContentClosed += () =>
                    {
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            Debug.Log("[AdManager] Rewarded Interstitial ad closed. Reloading...");
                            LoadRewardedInterstitialAd();
                        });
                    };
                });
        }

        public void ShowRewardedInterstitial(Action onRewardedComplete)
        {
            if (!_isAdMobEnabled || !IsNetworkConnected())
            {
                Debug.LogWarning("[AdManager] AdMob disabled or no internet. Bypassing ad to prevent lockup.");
                onRewardedComplete?.Invoke();
                return;
            }

            _onRewardedCallback = onRewardedComplete;

            if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
            {
                _rewardedInterstitialAd.Show((Reward reward) =>
                {
                    MobileAdsEventExecutor.ExecuteInUpdate(() =>
                    {
                        Debug.Log("[AdManager] User earned rewarded interstitial: " + reward.Amount + " " + reward.Type);
                        if (_onRewardedCallback != null)
                        {
                            _onRewardedCallback.Invoke();
                            _onRewardedCallback = null;
                        }
                    });
                });
            }
            else
            {
                Debug.LogWarning("[AdManager] Rewarded Interstitial ad not ready. Bypassing ad as fallback to prevent lockup.");
                onRewardedComplete?.Invoke();
                LoadRewardedInterstitialAd();
            }
        }
        #endregion
    }
}
