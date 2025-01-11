using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Networking-related functionalities for synchronizing data in the char select screen.
/// </summary>
public class GameStartNetworkBehaviour : NetworkBehaviour {
    private void Awake() {
        GameManager.Instance.playerManager.onPlayerSpawned += SubscibeToReadinessChanges;
        GameManager.Instance.playerManager.onPlayerDespawned += UnsubscibeToReadinessChanges;
    }

    public void SubscibeToReadinessChanges(Player player) {
        player.optionsChosen.OnValueChanged += OnAnyBoardReadinessChanged;
    }

    public void UnsubscibeToReadinessChanges(Player player) {
        player.optionsChosen.OnValueChanged -= OnAnyBoardReadinessChanged;
    }

    public void OnAnyBoardReadinessChanged(bool previous, bool current) {
        ServerStartIfAllReady();
    }

    public async void ServerStartIfAllReady() {
        // Only try to start the game if this is the server/host
        if (!NetworkManager.Singleton.IsServer) {
            Debug.LogError("Only the server can attempt to start the game!");
            return;
        }

        // Check that there are at least 2 players and there are no un-ready players
        int readyCount = 0;

        foreach (var player in GameManager.Instance.playerManager.players) {
            if (player.optionsChosen.Value) {
                readyCount++;
            } else {
                // a connected player is not ready. game can not start, stop checking
                return;
            }
        }

        // start after a delay if all connected players are ready and there are at least 2
        // in singleplayer, only wait for 1 player
        int requiredPlayers = GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.Singleplayer ? 1 : 2;
        if (readyCount >= requiredPlayers) {
            Debug.Log("All players ready - starting game after delay!");
            await Awaitable.WaitForSecondsAsync(0.5f);

            // Generate random battle data (seed) to be used for the upcoming battle, send this allong with the start game RPC
            BattleData battleData = new BattleData();
            battleData.cycleUniqueColors = 5;
            battleData.cycleLength = 7;
            battleData.Randomize();

            StartBattleRpc(battleData);

            NetworkManager.SceneManager.LoadScene("Battle", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void StartBattleRpc(BattleData battleData) {
        Debug.Log("Moving to battle scene");
        GameManager.Instance.battleData = battleData;
    }
}