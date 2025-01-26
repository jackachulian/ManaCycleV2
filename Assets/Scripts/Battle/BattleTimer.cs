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

    void Awake() {
        battleManager = GetComponent<BattleManager>();
        HideTimer();
        battleManager.onBattleStarted += ShowTimer;
    }

    void Update() {
        if (showing) {
            timerText.text = FormatTime(BattleManager.Instance.battleTime);
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
    static string FormatTime(float time, bool showDecimal = false)
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