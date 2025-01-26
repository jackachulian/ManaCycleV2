using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles sending and receiving of all RPCs needed for board synchronization.
/// I would put these on the board itself, and they were on there before,
/// but now Unity is being dumb about spawning them and soft synchronization isnt working how its supposed to
/// </summary>
public class PlayerBoardNetworkBehaviour : NetworkBehaviour {
    private Player player;

    void Awake() {
        player = GetComponent<Player>();
    }

    // ================ PieceManager.cs ================
    /// <summary>
    /// RPC to send to other clients when the position of the piece successfully changes.
    /// </summary>
    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    public void UpdateCurrentPieceRpc(Vector2Int position, int rotation) {
        player.battleInputHandler.board.pieceManager.UpdateCurrentPiece(position, rotation);
    }

    /// <summary>
    /// RPC to place the current piece and spawn the next one.
    /// </summary>
    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    public void PlaceCurrentPieceRpc() {
        player.battleInputHandler.board.pieceManager.PlaceCurrentPiece();
    }




    // ================ SpellcastManager.cs ================
    /// <summary>
    /// Called when a spellcast is first began.
    /// </summary>
    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    public void StartSpellcastRpc() {
        player.battleInputHandler.board.spellcastManager.StartSpellcast();
    }

    /// <summary>
    /// Called when a chain or cascade is triggered. Clear the current color, add to chain/cascade, and score appropriate amount of points
    /// </summary>
    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    public void SpellcastClearRpc() {
        player.battleInputHandler.board.spellcastManager.SpellcastClear();
    }

    /// <summary>
    /// End the current cascade and move to the next color in the chain
    /// </summary>
    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    public void EndCascadeRpc() {
        player.battleInputHandler.board.spellcastManager.EndCascade();
    }
    
    /// <summary>
    /// Call when the spellcast is no longer able to continue and the chain ends on a client's board via their timing
    /// </summary>
    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    public void EndChainRpc() {
        player.battleInputHandler.board.spellcastManager.EndChain();
    }


    // ================ HealthManager.cs ================
    /// <summary>
    /// Receive damage from another board.
    /// </summary>
    /// <param name="damage">the amount of damage dealt by the other player</param>
    [Rpc(SendTo.Everyone)]
    public void EnqueueDamageRpc(int damage) {
        player.battleInputHandler.board.healthManager.EnqueueDamage(damage);
    }
}