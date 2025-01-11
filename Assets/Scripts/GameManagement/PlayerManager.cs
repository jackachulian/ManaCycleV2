using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles the spawning, despawning and tracking of player objects and assigning IDs / retreiving by ID.
/// </summary>
public class PlayerManager : MonoBehaviour {
    const int playerLimit = 4;


    /// <summary>
    /// All players that are connected to the battle
    /// Index - int representing the board index of the player from 0-3
    /// Value - the player object, or null if no player with this ID
    /// ID assignment differs based on the implementation
    /// </summary>
    private List<Player> players = new List<Player>(4);

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

        // Change the owner of the char selector the player is about to control
        if (GameManager.Instance.currentGameState == GameManager.GameState.CharSelect) {
            var selectorNetworkObject = CharSelectManager.Instance.GetCharSelectorByIndex(boardIndex).GetComponent<NetworkObject>();
            if (selectorNetworkObject.IsSpawned) {
                selectorNetworkObject.ChangeOwnership(player.OwnerClientId);
            } else {
                selectorNetworkObject.SpawnWithOwnership(player.OwnerClientId);
            }
        }

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
        Debug.Log("Removed player with ID "+player.playerId.Value+" and board index "+player.boardIndex.Value);
    }

    /// <summary>
    /// To be called when the character scene is loaded from another scene. 
    /// Attaches players to selectors based on their current board index.
    /// </summary>
    public void AttachPlayersToSelectors() {
        Debug.Log("Attaching players to selectors");
        foreach (var player in players) {
            var selector = CharSelectManager.Instance.GetCharSelectorByIndex(player.boardIndex.Value);
            if (NetworkManager.Singleton.IsServer && selector) {
                var selectorNetworkObject = selector.GetComponent<NetworkObject>();
                if (selectorNetworkObject.OwnerClientId != player.OwnerClientId) {
                    if (selectorNetworkObject.IsSpawned) {
                        selectorNetworkObject.ChangeOwnership(player.OwnerClientId);
                    } else {
                        selectorNetworkObject.SpawnWithOwnership(player.OwnerClientId, false);
                    }
                }
            }

            player.AttachToCharSelector(selector);
        }
    }

    /// <summary>
    /// To be called when the battle scene is loaded from another scene.
    /// Attaches players to boards based on their current board index.
    /// </summary>
    public void AttachPlayersToBoards() {
        Debug.Log("Attaching players to boards");
        foreach (var player in players) {
            var board = BattleManager.Instance.GetBoardByIndex(player.boardIndex.Value);
            if (NetworkManager.Singleton.IsServer && board) {
                var boardNetworkObject = board.GetComponent<NetworkObject>();
                if (boardNetworkObject.OwnerClientId != player.OwnerClientId) {
                    if (boardNetworkObject.IsSpawned) {
                        boardNetworkObject.ChangeOwnership(player.OwnerClientId);
                    } else {
                        boardNetworkObject.SpawnWithOwnership(player.OwnerClientId);
                    }
                }
            }

            player.AttachToBattleBoard(board);
        }
    }

    public void EnableBattleInputs() {
        foreach (var player in players) {
            player.EnableBattleInputs();
        }
    }
}