using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Networking-related functionalities for synchronizing data in the char select screen.
/// </summary>
public class CharSelectNetworkBehaviour : NetworkBehaviour {
    private CharSelectManager charSelectManager;

    void Awake() {
        charSelectManager = GetComponent<CharSelectManager>();
        foreach (var selector in charSelectManager.charSelectors) {
            selector.onStateChanged += ServerStartIfAllReady;
        }
    }

    public async void ServerStartIfAllReady() {
        // Only try to start the game if this is the server/host
        if (!NetworkManager.Singleton.IsServer) return;

        // Check that there are at least 2 players and there are no un-ready players
        int readyCount = 0;

        foreach (var selector in charSelectManager.charSelectors) {
            if (selector.IsPlayerConnected()) {
                if (selector.IsReady()) {
                    readyCount++;
                } else {
                    // a connected player is not ready; stop checking
                    return;
                }
            }
        }

        // start after a delay if all connected players are ready and there are at least 2
        if (readyCount >= 2) {
            Debug.Log("All players ready - starting game after delay!");
            await Awaitable.WaitForSecondsAsync(0.5f);
            StartBattleRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    public void StartBattleRpc() {
        Debug.Log("Moving to battle scene");
        NetworkManager.SceneManager.LoadScene("Battle", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}