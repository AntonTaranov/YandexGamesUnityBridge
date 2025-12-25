var YandexGamesPlugin = {
    // Initialize Yandex Games SDK
    YandexGamesInitialize: function() {
        if (typeof YaGames === 'undefined') {
            console.error('Yandex Games SDK not found. Make sure to include YaGames script in your HTML.');
            SendMessage('YandexGamesCallbackReceiver', 'OnInitializeError', 'Yandex Games SDK not found');
            return;
        }
        
        if (!window.yandexGamesInitialized) {
            YaGames.init().then(ysdk => {
                window.ysdk = ysdk;
                window.yandexGamesInitialized = true;
                console.log('Yandex Games SDK initialized successfully');
                SendMessage('YandexGamesCallbackReceiver', 'OnInitialized', '');
            }).catch(err => {
                console.error('Failed to initialize Yandex Games SDK:', err);
                SendMessage('YandexGamesCallbackReceiver', 'OnInitializeError', err.message || 'Unknown error');
            });
        }
    },

    // Get player data
    GetPlayerDataAsyncJS: function() {
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnPlayerDataError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.getPlayer({ signed: true }).then(player => {
            var playerData = {
                name: player.getName() || 'Anonymous',
                avatar: player.getPhoto('medium') || '',
                lang: window.ysdk.environment.i18n.lang || 'en',
                device: window.ysdk.deviceInfo.type || 'desktop'
            };
            SendMessage('YandexGamesCallbackReceiver', 'OnPlayerDataReceived', JSON.stringify(playerData));
        }).catch(err => {
            console.error('Failed to get player data:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnPlayerDataError', err.message || 'Unknown error');
        });
    },

    // Save data to cloud storage
    SaveDataAsyncJS: function(keyPtr, dataPtr) {
        var key = UTF8ToString(keyPtr);
        var data = UTF8ToString(dataPtr);
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnSaveDataError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.getPlayer({ signed: true }).then(player => {
            var saveData = {};
            saveData[key] = data;
            return player.setData(saveData, true);
        }).then(() => {
            SendMessage('YandexGamesCallbackReceiver', 'OnSaveDataComplete', '');
        }).catch(err => {
            console.error('Failed to save data:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnSaveDataError', err.message || 'Unknown error');
        });
    },

    // Load data from cloud storage
    LoadDataAsyncJS: function(keyPtr) {
        var key = UTF8ToString(keyPtr);
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnLoadDataError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.getPlayer({ signed: true }).then(player => {
            return player.getData([key]);
        }).then(data => {
            var result = data[key] || null;
            SendMessage('YandexGamesCallbackReceiver', 'OnLoadDataComplete', result || '');
        }).catch(err => {
            console.error('Failed to load data:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnLoadDataError', err.message || 'Unknown error');
        });
    },

    // Show interstitial ad
    ShowInterstitialAdAsyncJS: function() {
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnInterstitialAdError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.adv.showFullscreenAdv({
            callbacks: {
                onClose: function(wasShown) {
                    SendMessage('YandexGamesCallbackReceiver', 'OnInterstitialAdComplete', '');
                },
                onError: function(error) {
                    console.error('Interstitial ad error:', error);
                    SendMessage('YandexGamesCallbackReceiver', 'OnInterstitialAdError', error.message || 'Unknown error');
                }
            }
        });
    },

    // Show rewarded ad
    ShowRewardedAdAsyncJS: function() {
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnRewardedAdError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.adv.showRewardedVideo({
            callbacks: {
                onRewarded: function() {
                    SendMessage('YandexGamesCallbackReceiver', 'OnRewardedAdComplete', 'true');
                },
                onClose: function() {
                    SendMessage('YandexGamesCallbackReceiver', 'OnRewardedAdComplete', 'false');
                },
                onError: function(error) {
                    console.error('Rewarded ad error:', error);
                    SendMessage('YandexGamesCallbackReceiver', 'OnRewardedAdError', error.message || 'Unknown error');
                }
            }
        });
    }
};

mergeInto(LibraryManager.library, YandexGamesPlugin);