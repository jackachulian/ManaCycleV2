using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

/// <summary>
/// A board that can be controlled by a player or an AI.
/// Manages the tile grid and movement/placement of pieces.
/// </summary>
public class Board : MonoBehaviour
{
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
    /// Set to true when initialized by the battlemanager.
    /// </summary>
    private bool initialized = false;

    /// <summary>
    /// Invoked when player is defeated.
    /// BattleManager will listen for this to know when to check when only one player is alive.
    /// </summary>
    public UnityEvent onDefeat {get; private set;} = new UnityEvent();

    /// <summary>
    /// Piece spawning will only happen while boardActive is true.
    /// Set to true when initialized, set to false when winning/losing.
    /// </summary>
    public bool boardActive {get; private set;}

    /// <summary>
    /// Set to true when this board either tops out or runs out of health.
    /// </summary>
    public bool defeated {get; private set;}

    /// <summary>
    /// Current player assigned. Set by BattleManager when players enter the battle scene.
    /// </summary>
    public Player player {get; private set;}

    public void SetPlayer(Player player) {
        this.player = player;
    }

    /// <summary>
    /// Called by BattleManager when battle is initialized
    /// Any initialization should go here (no Start() method)
    /// </summary>
    /// <param name="battleManager">the battle manager for the battle this board is being initialized within</param>
    /// <param name="seed">the seed to use for RNG</param>
    public void InitializeBattle(BattleManager battleManager, int seed) {
        if (initialized) {
            Debug.LogWarning(this+" already initialized; going to reinitialize but make sure this was intended!");
        } else {
            Debug.Log("Initializing battle board "+this);
        }

        defeated = false;
        boardActive = true;
        
        manaTileGrid = GetComponent<ManaTileGrid>();
        manaTileGrid.InitializeBattle();

        pieceManager = GetComponent<PieceManager>();
        pieceManager.InitializeBattle(this, seed);

        spellcastManager = GetComponent<SpellcastManager>();
        spellcastManager.InitializeBattle(this);

        healthManager = GetComponent<HealthManager>();
        healthManager.InitializeBattle(this);
    }

    public bool IsInitialized() {
        return initialized;
    }

    /// <summary>
    /// To be called when this player runs out of HP or tops out.
    /// Destroy the current piece, stop piece spawning, etc
    /// </summary>
    public void Defeat() {
        Debug.Log(this+" defeated!");
        boardActive = false;
        defeated = true;
        onDefeat.Invoke();
    }

    /// <summary>
    /// In singleplayer, called when player earns enough score / completes all objectives.
    /// In multiplayer, called when all other boards are defeated.
    /// </summary>
    public void Win() {
        Debug.Log(this+" won!");
        boardActive = false;
    }
}
