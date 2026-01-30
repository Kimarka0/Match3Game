mergeInto(LibraryManager.library, {

  YG_Init: function (unityObjectNamePtr) {
    const unityObjectName = UTF8ToString(unityObjectNamePtr);
    window.__yg_unityObjectName = unityObjectName;

    YaGames.init().then((ysdk) => {
      window.__ysdk = ysdk;

      ysdk.on('game_api_pause', () => {
        SendMessage(window.__yg_unityObjectName, 'OnGameApiPause', '');
      });

      ysdk.on('game_api_resume', () => {
        SendMessage(window.__yg_unityObjectName, 'OnGameApiResume', '');
      });

      SendMessage(window.__yg_unityObjectName, 'OnSdkInitialized', '1');
    }).catch((err) => {
      console.error('YaGames.init error', err);
      SendMessage(window.__yg_unityObjectName, 'OnSdkInitialized', '0');
    });
  },

  YG_GameReady: function () {
    if (!window.__ysdk) return;
    window.__ysdk.features.LoadingAPI.ready();
  },

  YG_GameplayStart: function () {
    if (!window.__ysdk) return;
    window.__ysdk.features.GameplayAPI.start();
  },

  YG_GameplayStop: function () {
    if (!window.__ysdk) return;
    window.__ysdk.features.GameplayAPI.stop();
  },

  YG_ShowInterstitial: function () {
    if (!window.__ysdk) return;

    window.__ysdk.adv.showFullscreenAdv({
      callbacks: {
        onOpen: () => {
          SendMessage(window.__yg_unityObjectName, 'OnInterstitialOpen', '1');
        },
        onClose: () => {
          SendMessage(window.__yg_unityObjectName, 'OnInterstitialClose', '1');
        },
        onError: (e) => {
          console.error('FullscreenAdv error', e);
          SendMessage(window.__yg_unityObjectName, 'OnInterstitialError', '1');
        }
      }
    });
  },

  YG_ShowRewarded: function () {
    if (!window.__ysdk) return;

    window.__ysdk.adv.showRewardedVideo({
      callbacks: {
        onOpen: () => {
          SendMessage(window.__yg_unityObjectName, 'OnRewardedOpen', '1');
        },
        onRewarded: () => {
          // награда засчиталась
          SendMessage(window.__yg_unityObjectName, 'OnRewarded', '1');
        },
        onClose: () => {
          // закрыли (и с наградой, и без)
          SendMessage(window.__yg_unityObjectName, 'OnRewardedClose', '1');
        },
        onError: (e) => {
          console.error('RewardedVideo error', e);
          SendMessage(window.__yg_unityObjectName, 'OnRewardedError', '1');
        }
      }
    });
  }
});
