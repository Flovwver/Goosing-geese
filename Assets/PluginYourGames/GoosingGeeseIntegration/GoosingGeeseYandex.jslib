mergeInto(LibraryManager.library, {
    $GoosingGeeseYandex: {
        pendingProfile: null,
        profileWriteActive: false,
        nextProfileWriteAt: 0,
        getProfileSession: function () {
            return typeof goosingGeeseProfile === "undefined" ? null : goosingGeeseProfile;
        },
        scheduleProfileWrite: function () {
            const state = GoosingGeeseYandex;
            const session = state.getProfileSession();

            if (state.profileWriteActive || !state.pendingProfile || !session || !session.player) {
                return;
            }

            state.profileWriteActive = true;
            setTimeout(async function () {
                const snapshot = state.pendingProfile;
                state.pendingProfile = null;

                try {
                    if (snapshot !== session.snapshot) {
                        state.nextProfileWriteAt = Date.now() + 3200;
                        await session.player.setData({ [goosingGeeseProfileKey]: snapshot }, true);
                        session.snapshot = snapshot;
                    }
                } catch (error) {
                    // The same snapshot is retained in local storage for a later retry.
                } finally {
                    state.profileWriteActive = false;
                    state.scheduleProfileWrite();
                }
            }, Math.max(0, state.nextProfileWriteAt - Date.now()));
        }
    },

    GoosingGeese_YandexHasSdk: function () {
        return typeof initYSDK !== "undefined" && initYSDK && typeof ysdk !== "undefined" && ysdk ? 1 : 0;
    },

    GoosingGeese_YandexReadProfile__deps: ["$GoosingGeeseYandex", "malloc", "$lengthBytesUTF8", "$stringToUTF8"],
    GoosingGeese_YandexReadProfile: function (readSnapshot) {
        const session = GoosingGeeseYandex.getProfileSession();
        const value = session && (readSnapshot ? session.snapshot : session.scope);

        if (!value) {
            return 0;
        }

        const size = lengthBytesUTF8(value) + 1;
        const buffer = _malloc(size);
        stringToUTF8(value, buffer, size);
        return buffer;
    },

    GoosingGeese_YandexWriteProfile__deps: ["$GoosingGeeseYandex"],
    GoosingGeese_YandexWriteProfile: function (jsonPtr) {
        const state = GoosingGeeseYandex;
        const session = state.getProfileSession();

        if (!session || !session.player) {
            return;
        }

        state.pendingProfile = UTF8ToString(jsonPtr);
        state.scheduleProfileWrite();
    }
});
