using System;
using System.Collections.Generic;
using Replay;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the spawning, despawning and tracking of player objects and assigning IDs / retreiving by ID.
/// </summary>
public class PlayerManager : MonoBehaviour {
    const int playerLimit = 4;

    public event Action<Player> onPlayerSpawned;
    public event Action<Player> onPlayerDespawned;

    /// <summary>
    /// Only used to spawn AI players
    /// </summary>
    [SerializeField]  private Player aiPlayerPrefab;

    /// <summary>
    /// Contains a list of all battlers.
    /// Used when replaying battlers to identi
    /// </summary>
    [SerializeField] private Battler[] battlerList;

    /// <summary>
    /// this is set up before a replay is started, used to assign battlers based on ReplayPlayer's battlerId
    /// </summary>
    private Dictionary<string, Battler> battlerTable = new Dictionary<string, Battler>();

    /// <summary>
    /// All players that are connected to the battle
    /// Index - int representing the board index of the player from 0-3
    /// Value - the player object, or null if no player with this ID
    /// ID assignment differs based on the implementation
    /// </summary>
    public List<Player> players {get; private set;} = new List<Player>(4);

    void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            AddCPUPlayer();
        }
    }


    /// <summary>
    /// Add a player to the player list and assign them a board index.
    /// </summary>
    /// <param name="player">the Player that was instantiated by the player connection manager</param>
    /// <param name="playerId">the playerId to assign to the new player</param>
    public void ServerAddPlayer(Player player) {
        // Server/host only
        if (!NetworkManager.Singleton.IsServer) {
            Debug.LogError("Only the server/host can add new players!");
            return;
        }

        // Raise an error if lobby is already at capacity
        if (players.Count >= playerLimit) {
            Debug.LogError("There is not enough slots for the new player to connect! Max 4 players.");
            return;
        }

        int boardIndex = players.Count;

        // Assign the board index to be the player's index in the list
        // With boardIndex's OnValueChanged callbacks, players will attach to their respective CharSelectors based on the index that is set by this server
        player.boardIndex.Value = boardIndex;

        Debug.Log("Spawned player with board index "+boardIndex);
    }

    /// <summary>
    /// Called by a Player's OnNetworkSpawn on clients (including the host).
    /// </summary>
    public void PlayerSpawned(Player player) {
        players.Add(player);
        onPlayerSpawned?.Invoke(player);
        Debug.Log("Added player with board index "+player.boardIndex.Value);
    }

    /// <summary>
    /// Remove the player object with the assigned ID.
    /// </summary>
    public void ServerRemovePlayer(Player player) {
        if (!NetworkManager.Singleton.IsServer) {
            Debug.LogError("Only the server/host can remove players!");
            return;
        }

        // there was code here but i removed it, nothing really needs to happen server-side yet when a player disconnects, just client side
    }

    /// <summary>
    /// Called by a Player's OnNetworkDespawn when it despawns on a client.
    /// </summary>
    public void PlayerDespawned(Player player) {
        players.Remove(player);
        onPlayerDespawned?.Invoke(player);
        Debug.Log("Removed player with ID "+player.playerId.Value+" and board index "+player.boardIndex.Value);
    }

    public void AddCPUPlayer() {
        // disabling this on the prefab will make it so the PlayerInputManager doesn't try to assign this a device when it is instantiated
        aiPlayerPrefab.GetComponent<PlayerInput>().enabled = false;
        aiPlayerPrefab.GetComponent<AIPlayerInput>().enabled = true;

        Player player = Instantiate(aiPlayerPrefab);
        player.isCpu = true;
        player.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
        player.DisableUserInput();
        player.username.Value = "CPU";
        player.EnableBattleAI();
    }

    public void AddReplayPlayers(ReplayData replayData) {
        PopulateBattlerTable();
        foreach (var replayPlayer in replayData.replayPlayers) {
            AddReplayPlayer(replayPlayer);
        }
    }

    private void AddReplayPlayer(ReplayPlayer replayPlayer) {
        aiPlayerPrefab.GetComponent<PlayerInput>().enabled = false;
        aiPlayerPrefab.GetComponent<AIPlayerInput>().enabled = false;

        Player player = Instantiate(aiPlayerPrefab);
        player.username.Value = replayPlayer.username;
        player.isCpu = replayPlayer.isCpu;
        if (battlerTable.ContainsKey(replayPlayer.battlerId)) {
            player.battler = battlerTable[replayPlayer.battlerId];
        } else {
            Debug.LogError("Battler with id "+replayPlayer.battlerId+" not found in PlayerManager's battle table");
        }

        player.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
        player.DisableUserInput();
    }

    public void PopulateBattlerTable() {
        foreach (Battler battler in battlerList) {
            if (battlerTable.ContainsKey(battler.battlerId)) {
                Debug.LogError("There is more than one battler with the id "+battler.battlerId);
            }
            battlerTable[battler.battlerId] = battler;
        }
    }

    /// <summary>
    /// To be called when the character scene is loaded from another scene. 
    /// Attaches players to selectors based on their current board index.
    /// </summary>
    public void AttachPlayersToSelectors() {
        Debug.Log("Attaching players to selectors");
        foreach (var player in players) {
            player.AttachToCharSelector();
        }
    }

    /// <summary>
    /// To be called when the battle scene is loaded from another scene.
    /// Attaches players to boards based on their current board index.
    /// </summary>
    public void AttachPlayersToBoards() {
        Debug.Log("Attaching players to boards");
        foreach (var player in players) {
            player.AttachToBattleBoard();
        }
    }

    public void EnableBattleInputs() {
        foreach (var player in players) {
            player.EnableBattleInputs();
        }
    }
}