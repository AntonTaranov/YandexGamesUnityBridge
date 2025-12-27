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
    },

    // Set leaderboard score
    SetLeaderboardScoreAsyncJS: function(leaderboardNamePtr, score, extraDataPtr) {
        var leaderboardName = UTF8ToString(leaderboardNamePtr);
        var extraData = extraDataPtr ? UTF8ToString(extraDataPtr) : '';
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnSetLeaderboardScoreError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.leaderboards.setScore(leaderboardName, score, extraData).then(() => {
            SendMessage('YandexGamesCallbackReceiver', 'OnSetLeaderboardScoreComplete', '');
        }).catch(err => {
            console.error('Failed to set leaderboard score:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnSetLeaderboardScoreError', err.message || 'Unknown error');
        });
    },

    // Get leaderboard description
    GetLeaderboardDescriptionAsyncJS: function(leaderboardNamePtr) {
        var leaderboardName = UTF8ToString(leaderboardNamePtr);
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardDescriptionError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.leaderboards.getDescription(leaderboardName).then(description => {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardDescriptionComplete', JSON.stringify(description));
        }).catch(err => {
            console.error('Failed to get leaderboard description:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardDescriptionError', err.message || 'Unknown error');
        });
    },

    // Get leaderboard player entry
    GetLeaderboardPlayerEntryAsyncJS: function(leaderboardNamePtr) {
        var leaderboardName = UTF8ToString(leaderboardNamePtr);
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardPlayerEntryError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.leaderboards.getPlayerEntry(leaderboardName).then(entry => {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardPlayerEntryComplete', JSON.stringify(entry));
        }).catch(err => {
            console.error('Failed to get leaderboard player entry:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardPlayerEntryError', err.message || 'Unknown error');
        });
    },

    // Get leaderboard entries
    GetLeaderboardEntriesAsyncJS: function(leaderboardNamePtr, optionsJson) {
        var leaderboardName = UTF8ToString(leaderboardNamePtr);
        var options = optionsJson ? JSON.parse(UTF8ToString(optionsJson)) : {};
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardEntriesError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.leaderboards.getEntries(leaderboardName, options).then(response => {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardEntriesComplete', JSON.stringify(response));
        }).catch(err => {
            console.error('Failed to get leaderboard entries:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardEntriesError', err.message || 'Unknown error');
        });
    },

    // Get remote config flags
    GetFlagsAsyncJS: function(optionsJson) {
        var options = optionsJson ? JSON.parse(UTF8ToString(optionsJson)) : {};
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetFlagsError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.getFlags(options).then(flags => {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetFlagsComplete', JSON.stringify(flags));
        }).catch(err => {
            console.error('Failed to get flags:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnGetFlagsError', err.message || 'Unknown error');
        });
    },

    // Check if review is available
    CanReviewAsyncJS: function() {
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnCanReviewError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.feedback.canReview().then(result => {
            SendMessage('YandexGamesCallbackReceiver', 'OnCanReviewComplete', JSON.stringify(result));
        }).catch(err => {
            console.error('Failed to check review availability:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnCanReviewError', err.message || 'Unknown error');
        });
    },

    // Request review from user
    RequestReviewAsyncJS: function() {
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnRequestReviewError', 'Yandex Games SDK not initialized');
            return;
        }

        window.ysdk.feedback.requestReview().then(result => {
            SendMessage('YandexGamesCallbackReceiver', 'OnRequestReviewComplete', JSON.stringify(result));
        }).catch(err => {
            console.error('Failed to request review:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnRequestReviewError', err.message || 'Unknown error');
        });
    }
};

mergeInto(LibraryManager.library, YandexGamesPlugin);