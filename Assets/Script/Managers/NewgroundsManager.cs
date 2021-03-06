using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using io.newgrounds.components.ScoreBoard;
using io.newgrounds;
using io.newgrounds.objects;
using io.newgrounds.results;
using System;
using io.newgrounds.results.Medal;

/// <summary>
/// https://bitbucket.org/newgrounds/newgrounds.io-for-unity-c/src/master/
/// </summary>
public class NewgroundsManager : Singleton<NewgroundsManager> {

    public enum MEDAL_TAG {
        DIE = 66574,
        CLEAR_SCREEN = 66575,
        P_25 = 66577,
        P_50 = 66578,
        P_100 = 66579,
        P_200 = 66580
    }

    public enum BOARD_TYPE {
        HIGHEST_SCORE,
        TOTAL_SCORE
    }
    private const int HIGHESTSCORE_BOARD_ID = 11148;

    private const int TOTALSCORE_BOARD_ID = 11162;

    [SerializeField]
    private core newgroundCore;

    [SerializeField]
    private bool isNewgroundsBuild;

    private bool isLoggedIn;

    private Dictionary<MEDAL_TAG, bool> unlockedMedals;

    void Start() {
        isLoggedIn = false;
        unlockedMedals = new Dictionary<MEDAL_TAG, bool>();

        if (!isNewgroundsBuild) {
            return;
        }
        CheckAndRequestLogin();
    }

    private bool ValidForNewgroundsAPI() {
        return isNewgroundsBuild && isLoggedIn;
    }

    #region Leaderboards


    public void AddScoreToLeaderboard(int scoreValue, BOARD_TYPE boardType) {

        if (!ValidForNewgroundsAPI()) {
            return;
        }

        postScore pScore = new postScore {
            id = GetBoardIDByType(),
            value = scoreValue,
            tag = Application.version
        };

        pScore.callWith(newgroundCore);

        #region Local_Function
        int GetBoardIDByType() {
            if (boardType == BOARD_TYPE.HIGHEST_SCORE) {
                return HIGHESTSCORE_BOARD_ID;
            }
            if (boardType == BOARD_TYPE.TOTAL_SCORE) {
                return TOTALSCORE_BOARD_ID;
            }
            Debug.LogWarning("Newgrounds Score Board ID for " + boardType.ToString() + " not found!");
            return 0;
        }
        #endregion
    }
    #endregion

    #region Medals

    public void UnlockMedal(MEDAL_TAG medalTag) {
        if (!ValidForNewgroundsAPI()) {
            return;
        }

        if (AlreadyUnlockedMedal()) {
            return;
        }

        io.newgrounds.components.Medal.unlock medal_unlock = new io.newgrounds.components.Medal.unlock();
        medal_unlock.id = (int) medalTag;
        medal_unlock.callWith(newgroundCore, onMedalUnlocked);

        #region Local_Function

        bool AlreadyUnlockedMedal() {
            if (!unlockedMedals.ContainsKey(medalTag)) {
                unlockedMedals.Add(medalTag, false);
            }
            return unlockedMedals[medalTag];
        }

        #endregion
    }

    // this will get called whenever a medal gets unlocked via unlockMedal()
    void onMedalUnlocked(io.newgrounds.results.Medal.unlock result) {
        medal medal = result.medal;
        Debug.Log("Medal Unlocked: " + medal.name + " (" + medal.value + " points)");

        MEDAL_TAG medalTag = (MEDAL_TAG)result.medal.id;
        if (!unlockedMedals.ContainsKey(medalTag)) {
            unlockedMedals.Add(medalTag, true);
        } else {
            unlockedMedals[medalTag] = true;
        }
    }

    #endregion

    #region LoginLogout

    void onLoggedIn() {
        user player = newgroundCore.current_user;
        isLoggedIn = true;
    }

    void onLoginFailed() {
        error error = newgroundCore.login_error;
    }

    void onLoginCancelled() {
    }

    void requestLogin() {
        newgroundCore.requestLogin(onLoggedIn, onLoginFailed, onLoginCancelled);
    }

    void cancelLogin() {
        newgroundCore.cancelLoginRequest();
    }

    void CheckAndRequestLogin() {
        newgroundCore.onReady(() => {

            // Call the server to check login status
            newgroundCore.checkLogin((bool logged_in) => {

                if (logged_in) {
                    onLoggedIn();
                } else {
                    requestLogin();
                }
            });
        });
    }
    #endregion
}
