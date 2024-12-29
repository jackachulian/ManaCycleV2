using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// A board that can be controlled by a player or an AI.
/// Manages the tile grid and movement/placement of pieces.
/// </summary>
public class Board : MonoBehaviour
{
    // ================ Non-Serialized Fields ================
    /// <summary>
    /// The battleManager that is managing this board. Also contains dependencies needed for SpawnPiece, etc
    /// Is set upon battle initialization.
    /// </summary>
    public BattleManager battleManager {get; private set;}

    /// <summary>
    /// Stores all placed mana tiles. Cached on initialization.
    /// </summary>
    public ManaTileGrid manaTileGrid {get; private set;}
    
    /// <summary>
    /// Manages the currently falling piece and spawning pieces. Cached on initialization.
    /// </summary>
    public PieceManager pieceManager {get; private set;}

    /// <summary>
    /// Manages spellcasts for this board. Cached on initialization.
    /// </summary>
    public SpellcastManager spellcastManager {get; private set;}

    /// <summary>
    /// Handles sending and receiving damage.
    /// </summary>
    public HealthManager healthManager {get; private set;}

    /// <summary>
    /// Called by BattleManager when battle is initialized
    /// Any initialization should go here (no Start() method)
    /// </summary>
    /// <param name="battleManager">the battle manager for the battle this board is being initialized within</param>
    /// <param name="seed">the seed to use for RNG</param>
    public void InitializeBattle(BattleManager battleManager, int seed) {
        this.battleManager = battleManager;
        
        manaTileGrid = GetComponent<ManaTileGrid>();
        manaTileGrid.InitializeBattle();

        pieceManager = GetComponent<PieceManager>();
        pieceManager.InitializeBattle(this, seed);

        spellcastManager = GetComponent<SpellcastManager>();
        spellcastManager.InitializeBattle(this);

        healthManager = GetComponent<HealthManager>();
        healthManager.InitializeBattle(this);
    }
}
