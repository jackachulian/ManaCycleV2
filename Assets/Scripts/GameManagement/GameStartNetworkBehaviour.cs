using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (GameManager.Instance.currentGameState == GameManager.GameState.CharSelect) ServerStartIfAllReady();
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
            ServerStartGame();
        }
    }

    /// <summary>
    /// Start game. Works wither from CharSelect or Battle scene.
    /// </summary>
    public void ServerStartGame() {
        // Generate random battle data (seed) to be used for the upcoming battle.
        BattleData battleData = new BattleData();
        battleData.cycleUniqueColors = 5;
        battleData.cycleLength = 7;
        battleData.Randomize();

        // send this to other players via an RPC, so that RNG and other per-battle data is sync'ed properly
        SetBattleDataRpc(battleData);

        NetworkManager.SceneManager.LoadScene("Battle", LoadSceneMode.Single);

        // Set all players unready so that the next char select will work properly after the upcoming battle
        foreach (var player in GameManager.Instance.playerManager.players) {
            player.selectedBattlerIndex.Value = -1;
            player.characterChosen.Value = false;
            player.optionsChosen.Value = false;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SetBattleDataRpc(BattleData battleData) {
        Debug.Log("Battle data set. RNG seed: "+battleData.seed);
        GameManager.Instance.battleData = battleData;
    }

    /// <summary>
    /// Request to go back to the character select screen.
    /// Can be sent from server or client. Only executes on server/host. Sends all players to the character select scene.
    /// </summary>
    [Rpc(SendTo.Owner)]
    public void GoToCharacterSelectRpc() {
        if (GameManager.Instance.currentGameState != GameManager.GameState.PostGame) {
            Debug.LogError("Game state must be PostGame to go back to character select");
            return;
        }

        GameManager.Instance.SetGameState(GameManager.GameState.CharSelect);
        
        NetworkManager.Singleton.SceneManager.LoadScene("CharSelect", LoadSceneMode.Single);
    }

    /// <summary>
    /// Request to go back to the character select screen.
    /// Can be sent from server or client. Only executes on server/host. Sends all players to the character select scene.
    /// </summary>
    [Rpc(SendTo.Owner)]
    public void RematchRpc() {
        if (GameManager.Instance.currentGameState != GameManager.GameState.PostGame) {
            Debug.LogError("Game state must be PostGame in order to start a rematch!");
            return;
        }

        ServerStartGame();
    }
}