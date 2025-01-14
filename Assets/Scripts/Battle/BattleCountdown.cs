using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Starts the battle, synchronized as closely as possible bewteen the clients.
/// </summary>
public class BattleCountdown : NetworkBehaviour {
    public bool countdownRunning {get; private set;} = false;

    public TMP_Text countdownText;

    public void StartCountdownServer() {
        if (!NetworkManager.Singleton.IsServer) {
            Debug.LogError("Only the server can start the countdown!");
            return;
        }

        float endTime = (float)NetworkManager.ServerTime.Time + 4.0f;

        Debug.Log("Countdown started on the server - end time: "+endTime);

        StartCoroutine(Countdown(endTime));
        CountdownClientRpc(endTime);
    }

    /// <summary>
    /// To start the synchronized countdown on clients other than the host
    /// </summary>
    [Rpc(SendTo.NotMe)]
    public void CountdownClientRpc(float endTime) {
        // subtract the local server time from the server's server time

        StartCoroutine(Countdown(endTime));
    }

    public IEnumerator Countdown(float countdownEndTime) {
        Debug.Log("Countdown started on the client - end time: "+countdownEndTime);

        countdownRunning = true;
        int lastSecondsDisplayed = 9999;
        countdownText.text = "";

        while (NetworkManager.ServerTime.Time < countdownEndTime) {
            int secondsRemaining = Mathf.CeilToInt(countdownEndTime - (float)NetworkManager.ServerTime.Time);
            if (secondsRemaining < lastSecondsDisplayed) {
                lastSecondsDisplayed = secondsRemaining;
                if (secondsRemaining <= 3) {
                    countdownText.text = secondsRemaining+"";
                }
            }
            yield return null;
        }

        countdownText.text = "Go!";
        countdownRunning = false;
        StartGameAfterCountdown();

        yield return new WaitForSeconds(1f);

        countdownText.text = "";
        countdownText.gameObject.SetActive(false);
    }

    public void StartGameAfterCountdown() {
        BattleManager.Instance.StartBattle();
    }
}