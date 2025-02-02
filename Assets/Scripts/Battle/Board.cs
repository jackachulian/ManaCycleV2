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
    public event Action onInitialized;

    /// <summary>
    /// Stores all placed mana tiles. Cached on initialization.
    /// </summary>
    public ManaTileGrid manaTileGrid {get; private set;}
    
    /// <summary>
    /// Manages the currently falling piece and spawning pieces. Cached on initialization.
    /// </summary>
    public PieceManager pieceManager {get; private set;}

    /// <summary>
    /// Manages the preview of where the pieceManager's current piece is going to land. Cached on initilaization.
    /// </summary>
    public GhostPieceManager ghostPieceManager {get; private set;}

    /// <summary>
    /// Cached UpcomingPieces component on the board. Manages the upcoming pieces list. Cached on initialization.
    /// </summary>
    [SerializeField] private UpcomingPieces _upcomingPieces;
    public UpcomingPieces upcomingPieces => _upcomingPieces;

    /// <summary>
    /// Manages spellcasts for this board. Cached on initialization.
    /// </summary>
    public SpellcastManager spellcastManager {get; private set;}

    /// <summary>
    /// Handles sending and receiving damage.
    /// </summary>
    public HealthManager healthManager {get; private set;}

    /// <summary>
    /// Handles gaining of score in singleplayer levels.
    /// </summary>
    public ScoreManager scoreManager {get; private set;}

    /// <summary>
    /// Handles building up SP and using spells.
    /// </summary>
    public SpellManager spellManager {get; private set;}

    /// <summary>
    /// Handles the battler portrait sprite and any other board-specific visual elements on the board.
    /// </summary>
    public BoardUI ui {get; private set;}

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

    /// <summary>
    /// Index of this board in the layout.
    /// Set by BoardLayout on its awake.
    /// </summary>
    public int boardIndex {get; set;}

    /// <summary>
    /// Materials used to display mana that is currently being cleared by a spellcast.
    /// The LitAMount is animated over time based on spellcast values.
    /// (Was moved from BattleManager to here because each board uses its own clear timing, oops.)
    /// </summary>
    public Material[] fadeGlowMaterials { get; private set; }

    public Material chromeFadeGlowMaterial { get; private set; }

    /// <summary>
    /// Cycle used only for this board. Only used in some layouts.
    /// </summary>
    [SerializeField] private ManaCycle _boardManaCycle;
    public ManaCycle boardManaCycle => _boardManaCycle;

    public void SetPlayer(Player player) {
        this.player = player;
        OnPlayerAssigned();
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
        boardActive = false; // will be set to true after countdown reaches 0

        // Generate fade glow materials
        var cosmetics = BattleManager.Instance.cosmetics;
        fadeGlowMaterials = new Material[cosmetics.manaVisuals.Length];
        for (int i = 0; i < cosmetics.manaVisuals.Length; i++)
        {
            ManaVisual visual = cosmetics.manaVisuals[i];
            fadeGlowMaterials[i] = new Material(visual.material);
        }

        chromeFadeGlowMaterial = new Material(cosmetics.chromeManaVisual.material);

        if (_boardManaCycle) _boardManaCycle.InitializeBattle(battleManager);

        manaTileGrid = GetComponent<ManaTileGrid>();
        ui = GetComponent<BoardUI>();
        spellcastManager = GetComponent<SpellcastManager>();
        ghostPieceManager = GetComponent<GhostPieceManager>();
        pieceManager = GetComponent<PieceManager>();
        healthManager = GetComponent<HealthManager>();
        scoreManager = GetComponent<ScoreManager>();
        spellManager = GetComponent<SpellManager>();

        ui.InitializeBattle(this);
        manaTileGrid.InitializeBattle();
        _upcomingPieces.InitializeBattle(this, seed);
        spellcastManager.InitializeBattle(this);
        ghostPieceManager.InitializeBattle(this);
        pieceManager.InitializeBattle(this);
        healthManager.InitializeBattle(this);
        scoreManager.InitializeBattle(this);
        spellManager.InitializeBattle(this);

        if (!player) {
            ui.ShowBattler(null);
            ui.HideBoard();
        }

        if (GameManager.Instance.level != null) {
            ListenForObjectives();
        }

        manaTileGrid.HideTiles(); // tiles will be shown when the game begins
        _upcomingPieces.HidePieces();

        onInitialized?.Invoke();
    }

    void OnDisable() {
        if (GameManager.Instance.level != null) {
            StopListeningForObjectives();
        }
    }

    public ManaCycle GetManaCycle()
    {
        return _boardManaCycle ? _boardManaCycle : BattleManager.Instance.manaCycle;
    }

    /// <summary>
    /// Called when the battle countdown reaches 0, and this board will start dropping pieces.
    /// </summary>
    public void StartBattle() {
        boardActive = true;
        manaTileGrid.ShowTiles();
        _upcomingPieces.ShowPieces();
    }

    /// <summary>
    /// Called when a player is assigned, OR unassigned for some reason.
    /// </summary>
    public void OnPlayerAssigned() {
        if (!ui) ui = GetComponent<BoardUI>();

        if (player) {
            ui.ShowBoard();
            if (ghostPieceManager && !ghostPieceManager.IsShowingGhostTiles()) ghostPieceManager.CreateGhostPiece();
        } else {
            ui.HideBoard();
        }

        if (player && player.battler) {
            ui.ShowBattler(player.battler);
        } else {
            ui.ShowBattler(null);
        }

        spellManager.OnPlayerAssigned();
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
        onDefeat?.Invoke();
        ui.ShowLoseText();
        ui.StartDefeatFall();
    }

    /// <summary>
    /// In singleplayer, called when player earns enough score / completes all objectives.
    /// In multiplayer, called when all other boards are defeated.
    /// </summary>
    public void Win() {
        Audio.AudioManager.Instance.PlayBoardSound("win", pitch: 1f);
        Debug.Log(this+" won!");
        boardActive = false;
        ui.ShowWinText();
    }

    bool listeningForObjectives = false;
    public void ListenForObjectives() {
        var level = GameManager.Instance.level;
        if (!level) return;

        if (listeningForObjectives) return;
        listeningForObjectives = true;

        // Check if every objective is completed
        foreach (var objective in level.objectiveList.objectives) {
            objective.onUpdated += CheckObjectivesCompleted;
        }
    }

    public void StopListeningForObjectives() {
        var level = GameManager.Instance.level;
        if (!level) return;

        if (!listeningForObjectives) return;
        listeningForObjectives = false;

        foreach (var objective in level.objectiveList.objectives) {
            objective.onUpdated -= CheckObjectivesCompleted;
        }
    }

    public void CheckObjectivesCompleted() {
        var level = GameManager.Instance.level;
        if (!level) return;

        // Check if every objective is completed
        foreach (var objective in level.objectiveList.objectives) {
            if (objective.GetProgress(this) < 1) {
                return;
            }
        }

        // Win the game :)
        BattleManager.Instance.EndBattle(winner: this);
    }
}
