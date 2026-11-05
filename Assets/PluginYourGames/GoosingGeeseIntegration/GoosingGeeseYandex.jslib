mergeInto(LibraryManager.library, {
    $GoosingGeeseYandex: {
        nextReadAt: 0,
        read: null,
        write: null,
        nextWriteAt: 0,
        getSdk: function () {
            return typeof ysdk === "undefined" ? null : ysdk;
        }
    },

    GoosingGeese_YandexLanguage__deps: ["$GoosingGeeseYandex"],
    GoosingGeese_YandexLanguage: function () {
        const sdk = GoosingGeeseYandex.getSdk();
        const language = sdk && sdk.environment && sdk.environment.i18n && sdk.environment.i18n.lang;
        return !language ? 0 : language === "ru" ? 1 : 2;
    },

    GoosingGeese_YandexReadLeaderboard__deps: ["$GoosingGeeseYandex"],
    GoosingGeese_YandexReadLeaderboard: function (boardPtr, idPtr, receiverPtr, requestId) {
        const sdk = GoosingGeeseYandex.getSdk();
        const receiver = UTF8ToString(receiverPtr);
        const localPlayerId = UTF8ToString(idPtr);
        const board = UTF8ToString(boardPtr);
        const state = GoosingGeeseYandex;
        const send = function (entries) {
            if (typeof ygGameInstance !== "undefined" && ygGameInstance) {
                ygGameInstance.SendMessage(receiver, "ReceiveLeaderboard", JSON.stringify({
                    RequestId: requestId,
                    Entries: entries
                }));
            }
        };

        if (!sdk || !sdk.leaderboards) {
            send(null);
            return;
        }

        if (!state.read || Date.now() >= state.nextReadAt) {
            state.nextReadAt = Date.now() + 16000;
            state.read = Promise.resolve().then(function () {
                return sdk.leaderboards.getEntries(board, { quantityTop: 5, includeUser: true, quantityAround: 1 });
            }).catch(function () {
                return null;
            });
        }

        state.read.then(function (result) {
            if (!result || !Array.isArray(result.entries)) {
                send(null);
                return;
            }

            send(result.entries.map(function (entry) {
                return {
                    PlayerId: entry.rank === result.userRank ? localPlayerId : "",
                    Name: (entry.player && entry.player.publicName) || "—",
                    Score: Math.min(entry.score, 2147483647),
                    Rank: entry.rank
                };
            }));
        });
    },

    GoosingGeese_YandexSubmitScore__deps: ["$GoosingGeeseYandex"],
    GoosingGeese_YandexSubmitScore: function (boardPtr, score) {
        const sdk = GoosingGeeseYandex.getSdk();
        const board = UTF8ToString(boardPtr);
        const state = GoosingGeeseYandex;

        if (!sdk || !sdk.leaderboards || score <= 0) {
            return;
        }

        state.write = (state.write || Promise.resolve()).then(async function () {
            if (!(await sdk.isAvailableMethod("leaderboards.setScore")) ||
                !(await sdk.isAvailableMethod("leaderboards.getPlayerEntry"))) {
                return;
            }

            try {
                const previous = await sdk.leaderboards.getPlayerEntry(board);

                if (previous.score >= score) {
                    return;
                }
            } catch (error) {
                if (error.code !== "LEADERBOARD_PLAYER_NOT_PRESENT") {
                    throw error;
                }
            }

            const delay = state.nextWriteAt - Date.now();

            if (delay > 0) {
                await new Promise(function (resolve) { setTimeout(resolve, delay); });
            }

            state.nextWriteAt = Date.now() + 1050;
            await sdk.leaderboards.setScore(board, score);
        }).catch(function () {
            // Local results remain available if the SDK request fails.
        });
    }
});
