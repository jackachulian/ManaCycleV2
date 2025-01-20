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
        player.onBattleDataReceived += CheckIfAllBattleDataReceived;
    }

    public void UnsubscibeToReadinessChanges(Player player) {
        player.optionsChosen.OnValueChanged -= OnAnyBoardReadinessChanged;
        player.onBattleDataReceived -= CheckIfAllBattleDataReceived;

    }

    public void OnAnyBoardReadinessChanged(bool previous, bool current) {
        if (GameManager.Instance.currentGameState == GameManager.GameState.CharSelect && NetworkManager.Singleton.IsServer) ServerStartIfAllReady();
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
            SendBattleDataServer();
        }
    }

    /// <summary>
    /// Start game. Works wither from CharSelect or Battle scene.
    /// </summary>
    public void SendBattleDataServer() {
        // Generate random battle data (seed) to be used for the upcoming battle.
        BattleData battleData = new BattleData();
        battleData.SetDefaults();
        battleData.Randomize();

        // send this to other players via an RPC, so that RNG and other per-battle data is sync'ed properly
        Debug.Log("Battle data set on server. RNG seed: "+battleData.seed);
        GameManager.Instance.SetBattleData(battleData);

        // in online mode, wait for other clients to receive battle data
        if (GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.OnlineMultiplayer) {
            foreach (var player in GameManager.Instance.playerManager.players) {
                player.onBattleDataReceived += CheckIfAllBattleDataReceived;
                if (!player.IsOwner) player.SetBattleDataClientRpc(battleData);
            }
        } else {
            StartGameServer();
        }
    }

    private void CheckIfAllBattleDataReceived(Player playerReceivedFrom, BattleData battleData) {
        if (battleData.seed != GameManager.Instance.battleData.seed) {
            Debug.LogError("BattleData received by player with boardIndex "+playerReceivedFrom.boardIndex.Value+" has seed "
                +battleData.seed+"! Expected seed: "+GameManager.Instance.battleData.seed);
            return;
        }

        // Wait until seed of all non-server clients matches the seed set by the server
        foreach (var player in GameManager.Instance.playerManager.players) {
            if (!player.IsOwner && player.receivedBattleData.seed != GameManager.Instance.battleData.seed) return;
        } 

        // If all match, game can now be started
        StartGameServer();
    }

    private void StartGameServer() {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += BattleManager.InstanceStartCountdownServer;
        GameManager.Instance.SetGameState(GameManager.GameState.Countdown);
        NetworkManager.SceneManager.LoadScene("Battle", LoadSceneMode.Single);
    }

    [Rpc(SendTo.Everyone)]
    /// <summary>
    /// Server/host will tell all clients to load the postgame menu once the server has decided the match is over and a winner has been decided.
    /// 
    /// </summary>
    public void PostgameMenuRpc(int winnerBoardIndex) {
        if (!BattleManager.Instance) {
            Debug.LogError("Trying to load postgame menu while not in the battle scene");
            return;
        }

        GameManager.Instance.SetGameState(GameManager.GameState.PostGame);

        Board winningBoard = BattleManager.Instance.GetBoardByIndex(winnerBoardIndex);
        BattleManager.Instance.ClientStartPostGame(winningBoard);
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

        SendBattleDataServer();
    }
}