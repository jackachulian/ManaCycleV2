using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Starts the battle, synchronized as closely as possible bewteen the clients.
/// </summary>
public class BattleTimer : MonoBehaviour {
    [SerializeField] private TMP_Text timerText;
    private BattleManager battleManager;

    bool showing = false;

    /// <summary>
    /// The time limit to clear the level.
    /// If time limit is 0, there is unlimited time, and the timer will count up from 0 instead of counting down the time.
    /// </summary>
    double timeLimit;

    void Awake() {
        battleManager = GetComponent<BattleManager>();
        HideTimer();
        battleManager.onBattleStarted += ShowTimer;
    }

    void Update() {
        if (showing) {
            double displayedTime;

            if (timeLimit > 0) {
                displayedTime = Math.Max(timeLimit - BattleManager.Instance.battleTime, 0);
                if (!BattleManager.Instance.gameCompleted && displayedTime <= 0) {
                    EndBattleTimeUp();
                }
            } else {
                displayedTime = BattleManager.Instance.battleTime;
            }

            timerText.text = FormatTime(displayedTime);
        }
    }

    /// <summary>
    /// When time runs out, defeat all boards.
    /// </summary>
    void EndBattleTimeUp() {
        if (!BattleManager.Instance.gameCompleted) {
            foreach (Board board in BattleManager.Instance.boardLayoutManager.currentLayout.boards) {
                if (!board.defeated && board.boardActive) {
                    board.Defeat();
                }
            }
        }
    }

    void HideTimer() {
        showing = false;
        timerText.text = "";
    }

    void ShowTimer() {
        showing = true;
        Update();
    }

    /// <summary>
    /// Format a time into a human readable format
    /// </summary>
    /// <param name="time">time in seconds, eother remaining time or elapsed time, depending on game</param>
    /// <param name="showDecimal">if true, will show some decimal places on the seconds</param>
    /// <returns>a human readable time string</returns>
    static string FormatTime(double time, bool showDecimal = false)
    {
        int minutes = (int)(time/60);
        int secondsRemainder = (int)(time % 60);
        int dec = (int)(time * 100 % 100);
        if (showDecimal) {
            return minutes + ":" + (secondsRemainder+"").PadLeft(2, '0')+"<size=40%>."+(dec+"").PadLeft(2, '0');
        } else {
            return minutes + ":" + (secondsRemainder+"").PadLeft(2, '0');
        }
    }
}