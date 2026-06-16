using Game.Core.UI;
using Game.States;
using Injection;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace Game.UI
{
    public sealed class EndGameHudMediator : Mediator<EndGameHud>
    {
        [Inject] private GameModel _gameModel;
        [Inject] private GameStateManager _gameStateManager;
        [Inject] private GameView _gameView;
        [Inject] private Game.Audio.SoundManager _soundManager;

        private readonly bool _isWin;

        public EndGameHudMediator(bool isWin)
        {
            _isWin = isWin;
        }

        protected override void Show()
        {
            _view.gameObject.SetActive(true);
            _view.RestartLabel = (_isWin) ? "Next" : "Restart";

            // Setup and animate result title
            _view.SetupResultTitle(_isWin);

            // Play victory or defeat sound
            if (_soundManager != null)
            {
                if (_isWin)
                {
                    _soundManager.PlayWinSound();
                }
                else
                {
                    _soundManager.PlayGameOverSound();
                }
            }

            // Attach hover and click animator to restart button
            if (_view.BtnRestart.GetComponent<UIButtonAnimator>() == null)
            {
                _view.BtnRestart.gameObject.AddComponent<UIButtonAnimator>();
            }

            // Animate restart button entry
            _view.BtnRestart.transform.localScale = Vector3.zero;
            _view.BtnRestart.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack).SetUpdate(true);

            _view.BtnRestart.onClick.AddListener(OnRestartClicked);

            // Check if user has coins in the HUD to double via Rewarded Ad (Victory) or needs a Revive (Defeat due to TimeOut)
            if (_isWin)
            {
                int coinsCollected = _gameView.GameHud.Coins;
                if (coinsCollected > 0)
                {
                    _view.SetupRewardedButton("Watch Ad (Double Coins)", OnWatchDoubleCoinsRewardedAd);
                }
                else
                {
                    _view.HideRewardedButton();
                }
            }
            else
            {
                // Defeat screen
                if (_gameStateManager.Current is GamePlayHideState)
                {
                    // Hide Mode (Employee): player got caught, show Revive ad button
                    _view.SetupRewardedButton("Watch Ad (Revive)", OnWatchReviveRewardedAd);
                }
                else if (_gameModel.IsTimeOut)
                {
                    // Seek Mode (Boss): running out of time, show +30s ad button
                    _view.SetupRewardedButton("Watch Ad (+30s Time)", OnWatchExtraTimeRewardedAd);
                }
                else
                {
                    _view.HideRewardedButton();
                }
            }

            // Show AdMob Interstitial ad on game over
            if (Game.Ads.AdManager.Instance != null)
            {
                Game.Ads.AdManager.Instance.ShowInterstitial();
            }
        }

        private void OnWatchDoubleCoinsRewardedAd()
        {
            if (Game.Ads.AdManager.Instance != null)
            {
                Game.Ads.AdManager.Instance.ShowRewardedInterstitial(() =>
                {
                    // Success! Double coins in HUD
                    _gameView.GameHud.Coins *= 2;
                    _view.HideRewardedButton();
                    Debug.Log("[EndGameHudMediator] Rewarded Interstitial Ad watched! Coins doubled.");
                });
            }
        }

        private void OnWatchExtraTimeRewardedAd()
        {
            if (Game.Ads.AdManager.Instance != null)
            {
                Game.Ads.AdManager.Instance.ShowRewarded(() =>
                {
                    // Success! Revive player with +30 seconds
                    var seekState = _gameStateManager.Current as GamePlaySeekState;
                    if (seekState != null)
                    {
                        seekState.Revive(30f);
                    }
                    Unmediate();
                    Debug.Log("[EndGameHudMediator] Rewarded Ad watched! Player revived with +30s.");
                });
            }
        }

        private void OnWatchReviveRewardedAd()
        {
            if (Game.Ads.AdManager.Instance != null)
            {
                Game.Ads.AdManager.Instance.ShowRewarded(() =>
                {
                    // Success! Revive player in Hide Mode (Employee slacker)
                    var hideState = _gameStateManager.Current as GamePlayHideState;
                    if (hideState != null)
                    {
                        hideState.Revive();
                    }
                    Unmediate();
                    Debug.Log("[EndGameHudMediator] Rewarded Ad watched! Player revived in Hide Mode.");
                });
            }
        }
        
        protected override void Hide()
        {
            if (_view != null)
            {
                _view.gameObject.SetActive(false);
                _view.BtnRestart.onClick.RemoveAllListeners();
                _view.HideRewardedButton();
            }
        }

        private void OnRestartClicked()
        {
            NextLevel(_isWin);
        }

        public void NextLevel(bool isWin)
        {
            if (isWin)
            {
                // Save max unlocked level progression
                int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
                if (_gameModel.Level >= maxUnlocked)
                {
                    PlayerPrefs.SetInt("MaxUnlockedLevel", _gameModel.Level + 1);
                    PlayerPrefs.Save();
                }

                // Advance to next level
                _gameModel.Level++;
            }

            _gameModel.Save();

            bool isSceneExists = _gameModel.Level < SceneManager.sceneCountInBuildSettings;
            if (!isSceneExists)
            {
                _gameModel.Remove();
            }

            _gameStateManager.SwitchToState(new GameUnloadState());
        }
    }
}