var YandexGamesPlugin = {
    // State tracking for idempotency (T001)
    _gameReady: false,
    _gameplayActive: false,
    
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

        window.ysdk.getPlayer().then(player => {
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
            SendMessage('YandexGamesCallbackReceiver', 'OnSaveDataError', JSON.stringify({key: key, error: 'Yandex Games SDK not initialized'}));
            return;
        }

        window.ysdk.getPlayer().then(player => {
            var saveData = {};
            saveData[key] = data;
            return player.setData(saveData, true);
        }).then(() => {
            SendMessage('YandexGamesCallbackReceiver', 'OnSaveDataComplete', key);
        }).catch(err => {
            console.error('Failed to save data:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnSaveDataError', JSON.stringify({key: key, error: err.message || 'Unknown error'}));
        });
    },

    // Load data from cloud storage
    LoadDataAsyncJS: function(keyPtr) {
        var key = UTF8ToString(keyPtr);
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnLoadDataError', JSON.stringify({key: key, error: 'Yandex Games SDK not initialized'}));
            return;
        }

        window.ysdk.getPlayer().then(player => {
            return player.getData([key]);
        }).then(data => {
            var result = data[key] || null;
            SendMessage('YandexGamesCallbackReceiver', 'OnLoadDataComplete', JSON.stringify({key: key, data: result || ''}));
        }).catch(err => {
            console.error('Failed to load data:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnLoadDataError', JSON.stringify({key: key, error: err.message || 'Unknown error'}));
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
            SendMessage('YandexGamesCallbackReceiver', 'OnSetLeaderboardScoreError', JSON.stringify({leaderboardName: leaderboardName, error: 'Yandex Games SDK not initialized'}));
            return;
        }

        window.ysdk.leaderboards.setScore(leaderboardName, score, extraData).then(() => {
            SendMessage('YandexGamesCallbackReceiver', 'OnSetLeaderboardScoreComplete', leaderboardName);
        }).catch(err => {
            console.error('Failed to set leaderboard score:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnSetLeaderboardScoreError', JSON.stringify({leaderboardName: leaderboardName, error: err.message || 'Unknown error'}));
        });
    },

    // Get leaderboard description
    GetLeaderboardDescriptionAsyncJS: function(leaderboardNamePtr) {
        var leaderboardName = UTF8ToString(leaderboardNamePtr);
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardDescriptionError', JSON.stringify({leaderboardName: leaderboardName, error: 'Yandex Games SDK not initialized'}));
            return;
        }

        window.ysdk.leaderboards.getDescription(leaderboardName).then(description => {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardDescriptionComplete', JSON.stringify({leaderboardName: leaderboardName, data: description}));
        }).catch(err => {
            console.error('Failed to get leaderboard description:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardDescriptionError', JSON.stringify({leaderboardName: leaderboardName, error: err.message || 'Unknown error'}));
        });
    },

    // Get leaderboard player entry
    GetLeaderboardPlayerEntryAsyncJS: function(leaderboardNamePtr) {
        var leaderboardName = UTF8ToString(leaderboardNamePtr);
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardPlayerEntryError', JSON.stringify({leaderboardName: leaderboardName, error: 'Yandex Games SDK not initialized'}));
            return;
        }

        window.ysdk.leaderboards.getPlayerEntry(leaderboardName).then(entry => {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardPlayerEntryComplete', JSON.stringify({leaderboardName: leaderboardName, data: entry}));
        }).catch(err => {
            console.error('Failed to get leaderboard player entry:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardPlayerEntryError', JSON.stringify({leaderboardName: leaderboardName, error: err.message || 'Unknown error'}));
        });
    },

    // Get leaderboard entries
    GetLeaderboardEntriesAsyncJS: function(leaderboardNamePtr, optionsJson) {
        var leaderboardName = UTF8ToString(leaderboardNamePtr);
        var optionsStr = optionsJson ? UTF8ToString(optionsJson) : '{}';
        var options = JSON.parse(optionsStr);
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardEntriesError', JSON.stringify({requestKey: leaderboardName + '_' + optionsStr, error: 'Yandex Games SDK not initialized'}));
            return;
        }

        window.ysdk.leaderboards.getEntries(leaderboardName, options).then(response => {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardEntriesComplete', JSON.stringify({requestKey: leaderboardName + '_' + optionsStr, data: response}));
        }).catch(err => {
            console.error('Failed to get leaderboard entries:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnGetLeaderboardEntriesError', JSON.stringify({requestKey: leaderboardName + '_' + optionsStr, error: err.message || 'Unknown error'}));
        });
    },

    // Get remote config flags
    GetFlagsAsyncJS: function(optionsJson) {
        var optionsStr = optionsJson ? UTF8ToString(optionsJson) : '{}';
        var options = JSON.parse(optionsStr);
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetFlagsError', JSON.stringify({requestKey: optionsStr, error: 'Yandex Games SDK not initialized'}));
            return;
        }

        window.ysdk.getFlags(options).then(flags => {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetFlagsComplete', JSON.stringify({requestKey: optionsStr, data: flags}));
        }).catch(err => {
            console.error('Failed to get flags:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnGetFlagsError', JSON.stringify({requestKey: optionsStr, error: err.message || 'Unknown error'}));
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
    },

    // Get product catalog
    GetCatalogAsyncJS: function() {
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetCatalogError', 'Yandex Games SDK not initialized');
            return;
        }

        // Lazy initialize payments
        if (!window.yandexPayments) {
            window.yandexPayments = window.ysdk.getPayments({ signed: false });
        }

        window.yandexPayments.then(payments => {
            return payments.getCatalog();
        }).then(catalog => {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetCatalogComplete', JSON.stringify(catalog));
        }).catch(err => {
            console.error('Failed to get catalog:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnGetCatalogError', err.message || 'Unknown error');
        });
    },

    // Purchase product
    PurchaseAsyncJS: function(productIdPtr, developerPayloadPtr) {
        var productId = UTF8ToString(productIdPtr);
        var developerPayload = developerPayloadPtr ? UTF8ToString(developerPayloadPtr) : '';
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnPurchaseError', 'Yandex Games SDK not initialized');
            return;
        }

        if (!window.yandexPayments) {
            window.yandexPayments = window.ysdk.getPayments({ signed: false });
        }

        window.yandexPayments.then(payments => {
            var options = { id: productId };
            if (developerPayload) {
                options.developerPayload = developerPayload;
            }
            return payments.purchase(options);
        }).then(purchase => {
            SendMessage('YandexGamesCallbackReceiver', 'OnPurchaseComplete', JSON.stringify(purchase));
        }).catch(err => {
            console.error('Failed to purchase:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnPurchaseError', err.message || 'Unknown error');
        });
    },

    // Get unconsumed purchases
    GetPurchasesAsyncJS: function() {
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetPurchasesError', 'Yandex Games SDK not initialized');
            return;
        }

        if (!window.yandexPayments) {
            window.yandexPayments = window.ysdk.getPayments({ signed: false });
        }

        window.yandexPayments.then(payments => {
            return payments.getPurchases();
        }).then(purchasesResponse => {
            SendMessage('YandexGamesCallbackReceiver', 'OnGetPurchasesComplete', JSON.stringify(purchasesResponse));
        }).catch(err => {
            console.error('Failed to get purchases:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnGetPurchasesError', err.message || 'Unknown error');
        });
    },

    // Consume purchase
    ConsumePurchaseAsyncJS: function(tokenPtr) {
        var purchaseToken = UTF8ToString(tokenPtr);
        
        if (!window.ysdk) {
            SendMessage('YandexGamesCallbackReceiver', 'OnConsumePurchaseError', 'Yandex Games SDK not initialized');
            return;
        }

        if (!window.yandexPayments) {
            window.yandexPayments = window.ysdk.getPayments({ signed: false });
        }

        window.yandexPayments.then(payments => {
            return payments.consumePurchase(purchaseToken);
        }).then(() => {
            SendMessage('YandexGamesCallbackReceiver', 'OnConsumePurchaseComplete', '');
        }).catch(err => {
            console.error('Failed to consume purchase:', err);
            SendMessage('YandexGamesCallbackReceiver', 'OnConsumePurchaseError', err.message || 'Unknown error');
        });
    },

    // T002: LoadingAPI.ready() - Signal game loading complete
    LoadingAPIReady: function() {
        if (this._gameReady) {
            console.warn('[YandexGames] LoadingAPI.ready() already called - ignoring duplicate call');
            return;
        }
        
        if (window.ysdk && window.ysdk.features && window.ysdk.features.LoadingAPI) {
            try {
                window.ysdk.features.LoadingAPI.ready();
                this._gameReady = true;
                console.log('[YandexGames] LoadingAPI.ready() called successfully');
            } catch (error) {
                console.error('[YandexGames] Error calling LoadingAPI.ready():', error);
            }
        } else {
            console.warn('[YandexGames] LoadingAPI not available - SDK may not be initialized');
        }
    },

    // T003: GameplayAPI.start() - Signal gameplay started/resumed
    GameplayAPIStart: function() {
        if (this._gameplayActive) {
            console.log('[YandexGames] GameplayAPI.start() already active - ignoring duplicate call');
            return;
        }
        
        if (window.ysdk && window.ysdk.features && window.ysdk.features.GameplayAPI) {
            try {
                window.ysdk.features.GameplayAPI.start();
                this._gameplayActive = true;
                console.log('[YandexGames] GameplayAPI.start() called successfully');
            } catch (error) {
                console.error('[YandexGames] Error calling GameplayAPI.start():', error);
            }
        } else {
            console.warn('[YandexGames] GameplayAPI not available - SDK may not be initialized');
        }
    },

    // T004: GameplayAPI.stop() - Signal gameplay stopped/paused
    GameplayAPIStop: function() {
        if (!this._gameplayActive) {
            console.log('[YandexGames] GameplayAPI.stop() already stopped - ignoring duplicate call');
            return;
        }
        
        if (window.ysdk && window.ysdk.features && window.ysdk.features.GameplayAPI) {
            try {
                window.ysdk.features.GameplayAPI.stop();
                this._gameplayActive = false;
                console.log('[YandexGames] GameplayAPI.stop() called successfully');
            } catch (error) {
                console.error('[YandexGames] Error calling GameplayAPI.stop():', error);
            }
        } else {
            console.warn('[YandexGames] GameplayAPI not available - SDK may not be initialized');
        }
    }
};

mergeInto(LibraryManager.library, YandexGamesPlugin);