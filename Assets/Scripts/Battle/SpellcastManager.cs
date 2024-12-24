using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component to be used on the same gameobject as the Board.
/// Handles spellcasting and the timing associated with it.
/// </summary>
public class SpellcastManager : MonoBehaviour {
    // ================ Serialized fields ================
    /// <summary>
    /// Minimum possible time between clears before a cascade can be triggered. 
    /// Cascade can be triggered anytime after this delay and before chain delay reaches 0.
    /// </summary>
    [SerializeField] private float cascadeDelay = 0.5f;

    /// <summary>
    /// Minimum possible time bewteen clears before a chain can be triggered.
    /// Chain can be triggered anytime after this delay and before maxChainDelay.
    /// </summary>
    [SerializeField] private float chainDelay = 0.75f;
    
    /// <summary>
    /// Maximum amount of allotted time new blobs can still be created and cleared before a chain dies and the combo ends.
    /// </summary>
    [SerializeField] private float maxChainDelay = 2.00f;

    /// <summary>
    /// CyclePointer to position on the current color being cleared
    /// </summary>
    [SerializeField] private Transform cyclePointer;

    /// <summary>
    /// Amount of units of offset to position the cycle pointer.
    /// </summary>
    [SerializeField] private float cyclePointerOffset = 1.5f;

    private enum BoardSide {
        LEFT,
        RIGHT
    }
    /// <summary>
    /// The side of the screen the board is on. Used for cycle pointer positioning.
    /// </summary>
    [SerializeField] private BoardSide boardSide = BoardSide.LEFT;

    // ================ Non-serialized fields ================
    // ======== General ========
    /// <summary>
    /// The Board this is managing the spellcasts of. Cached on InitializeBattle()
    /// </summary>
    private Board board;

    /// <summary>
    /// This board's current position in the cycle's color sequence
    /// </summary>
    private int cycleIndex;

    // ======== Blob detection ========
    /// <summary>
    /// Contains a grid of all tiles and the blob they have been added to, or null for no blob. Used during connected tile searching.
    /// </summary>
    private List<Vector2Int>[,] blobGrid;

    /// <summary>
    /// The total amount of clearable mana on the board for each color, as discovered from the last blob refresh.
    /// Used to determine if a spellcast can currently be initiated. If current color clearable mana is 0, a buzzer sound will play and no spellcast will occur.
    /// </summary>
    public int[] clearableManaCounts;

    /// <summary>
    /// Minimum blob size required to clear
    /// </summary>
    public readonly int minBlobSize = 3;

    // ======== Spellcasting ========
    /// <summary>
    /// Whether or not a spellcast is currently in progress.
    /// </summary>
    public bool spellcasting {get; private set;}

    // TODO: Make these private!! only public for debugging purposes
    /// <summary>
    /// Current cascade off the current color during a spellcast.
    /// Set to 0 when a spellcast is not occuring.
    /// If after a clear, the same color is clearable again, this is set to 1.
    /// If still clearable after cascadeDelay, trigger a cascade clear and increment by 1.
    /// Once cascade ends set back to 0.
    /// </summary>
    public int currentCascade;

    /// <summary>
    /// Current chain during a spellcast
    /// </summary>
    public int currentChain;

    /// <summary>
    /// Is set to 0 when a clear happens during a spellcast and counts upward.
    /// Used for spellcast cascade/chain timing.
    /// </summary>
    public float timeSinceLastClear;

    
    // ================ Methods ================
    /// <summary>
    /// Called when the battle initializes, after the ManaCycle and the Board for this spellcastmanager is initialized.
    /// </summary>
    /// <param name="board"></param>
    public void InitializeBattle(Board board) {
        this.board = board;
        blobGrid = new List<Vector2Int>[board.manaTileGrid.width, board.manaTileGrid.height];
        clearableManaCounts = new int[board.battleManager.manaCycle.cycleUniqueColors];

        RepositionCyclePointer();
    }

    void Update() {
        SpellcastUpdate();
    }

    /// <summary>
    /// Updates the state of the spellcast based on several timing variables and the changing state of the board.
    /// Runs each frame.
    /// </summary>
    void SpellcastUpdate() {
        if (!spellcasting) return;

        timeSinceLastClear += Time.deltaTime;

        // Check to see if there is a valid cascade
        if (
            currentCascade > 0   // current cascade must be 1 or higher, meaning that a cascade was possible the instant after the last clear happened.
            && timeSinceLastClear >= cascadeDelay   // must happen after cascade delay
            && timeSinceLastClear < chainDelay      // must happen before chain delay
            && IsCurrentColorClearable()            // current color in cycle must be clearable
        ) {
            SpellcastClear();
        } 
        // If a cascade is ongoing, but there is not a valid cascade and cascade period has passed (chain delay reached), 
        // end the cascade and move to the next color in the chain.
        // note: this will only be reached if a cascade is set to happen right after a clear, but an ability tile breaks the cascade before cascadeDelay passes
        else if (currentCascade > 0 && timeSinceLastClear >= chainDelay) {
            currentCascade = 0;
            AdvanceCycle();
        }

        // Check to see if there is a valid chain
        if (
            timeSinceLastClear >= chainDelay    // must happen after chain delay
            && timeSinceLastClear < maxChainDelay   // must happen before max chain delay
            && IsCurrentColorClearable()            // current color in cycle must be clearable
        ) {
            SpellcastClear();
        } 
        // otherwise, if past max chain delay, end the chain here
        else if (timeSinceLastClear >= maxChainDelay) {
            spellcasting = false;
            currentChain = 0;
            currentCascade = 0;
            Debug.Log("Chain ended");
        }
    }

    /// <summary>
    /// Called when a chain or cascade is triggered. Clear the current color, add to chain/cascade, and score appropriate amount of points
    /// </summary>
    private void SpellcastClear() {
        // if a cascade is happening, add to cascade, otherwise add to chain
        if (currentCascade > 0) {
            Debug.Log("Cascade clearing - chain="+currentChain+", cascade="+currentCascade+", color="+GetCurrentCycleColor());
            currentCascade += 1;
        } else {
            Debug.Log("Chain clearing - chain="+currentChain+", cascade="+currentCascade+", color="+GetCurrentCycleColor());
            currentChain += 1;
        }

        // Clear the current cycle color and recalculate connected tiles
        ClearColor(GetCurrentCycleColor());
        board.manaTileGrid.AllTileGravity();
        RefreshBlobs();

        // If the next color is immediately clearable, start a cascade if not already cascading, or leave the current cascade continuing
        if (IsCurrentColorClearable()) {
            if (currentCascade == 0) currentCascade = 1;
        }
        // otherwise, advance to the next color in the cycle
        else {
            AdvanceCycle();
        }
        
        timeSinceLastClear = 0;
    }

    /// <summary>
    /// Move this board's pointer to the next sequential color in the cycle.
    /// </summary>
    private void AdvanceCycle() {
        cycleIndex += 1;
        if (cycleIndex >= board.battleManager.manaCycle.cycleLength) {
            cycleIndex = 0;
            // TODO: cycle multiplier
        }

        RepositionCyclePointer();
    }

    private void RepositionCyclePointer() {
        var cycleManaTile = board.battleManager.manaCycle.GetCycleTile(cycleIndex);
        
        // TODO: in 2-player mode, player 2's pointer is offset to the right instead of left. 
        // in 3 and 4-player mode, half and half on each side and handle overlaps by spreading out the sprites slightly
        Vector3 offsetDirection = boardSide == BoardSide.LEFT ? Vector3.left : Vector3.right;
        cyclePointer.position = cycleManaTile.transform.position + offsetDirection * cyclePointerOffset;
    }

    /// <summary>
    /// Try to perform a spellcast.
    /// </summary>
    public void TrySpellcast() {
        if (clearableManaCounts[GetCurrentCycleColor()] <= 0) {
            // TODO: shake pointer, buzzer sound
            Debug.Log("Can't spellcast!");
            return;
        }

        // TODO: play spellcast startup sound
        spellcasting = true;
        timeSinceLastClear = 0;
        currentCascade = 0;
        currentChain = 0;
        Debug.Log("Spellcast has begun!");
    }

    /// <summary>
    /// Returns the color that this board is currently on and has to clear in order to advance the cycle.
    /// </summary>
    /// <returns>the int representing the current color</returns>
    public int GetCurrentCycleColor() {
        return board.battleManager.manaCycle.GetSequenceColor(cycleIndex);
    }

    /// <summary>
    /// Returns true if, according to the last blob update, the current cycle color can be cleared.
    /// </summary>
    /// <returns>true if current cycle color is currently clearable</returns>
    public bool IsCurrentColorClearable() {
        return clearableManaCounts[GetCurrentCycleColor()] > 0;
    }

    /// <summary>
    /// Rebuild all blobs. Should be called after the grid has changed.
    /// </summary>
    public void RefreshBlobs() {
        for (int y = 0; y < board.manaTileGrid.height; y++) {
            for (int x = 0; x < board.manaTileGrid.width; x++) {
                blobGrid[x,y] = null;
            }
        }

        Array.Fill(clearableManaCounts, 0);

        for (int y = 0; y < board.manaTileGrid.height; y++)
        {
            for (int x = 0; x < board.manaTileGrid.width; x++)
            {
                // if this tile was already added to a blob, skip
                List<Vector2Int> blob = blobGrid[x, y];
                if (blob != null) continue;

                // Check if there is a tile here. If so, try to add it to a blob
                Vector2Int position = new Vector2Int(x, y);
                ManaTile tile = board.manaTileGrid.GetTile(position);

                // If there's a tile here, try building a blob off this tile.
                if (tile) {
                    blob = new List<Vector2Int>();
                    int color = tile.color;
                    ExpandBlob(blob, position, color);

                    // if blob is clearable, add to the clearable mana couns
                    if (blob.Count >= minBlobSize) {
                        clearableManaCounts[color] += blob.Count;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Recursively expands the passed blob to all connected tiles
    /// </summary>
    /// <param name="blob">the blob (list of vecor2ints) to add to</param>
    /// <param name="position">position of the possible tile to try to add to the blob</param>
    /// <param name="color">the color to check for </param>
    void ExpandBlob(List<Vector2Int> blob, Vector2Int position, int color)
    {
        // Don't add to blob if the tile is in an invalid position
        if (!board.manaTileGrid.IsInBounds(position)) return;

        // Don't add to blob if already in this blob or another blob; this would cause an infinite loop
        if (blobGrid[position.x, position.y] != null) return;

        // Don't add if there is not a tile here
        ManaTile tile = board.manaTileGrid.GetTile(position);
        if (tile == null) return;

        // Don't add if the tile is the incorrect color
        if (tile.color != color) return;

        // Add the tile to the blob and fill in its spot on the blob matrix
        blob.Add(position);
        blobGrid[position.x, position.y] = blob;

        // Expand out the current blob on all sides, checking for the same colored tile to add to this blob
        ExpandBlob(blob, position + Vector2Int.left, color);
        ExpandBlob(blob, position + Vector2Int.right, color);
        ExpandBlob(blob, position + Vector2Int.up, color);
        ExpandBlob(blob, position + Vector2Int.down, color);
    }

    /// <summary>
    /// Clear all blobs of the specified color.
    /// Blob size must be at least <c>minBlobSize</c>.
    /// </summary>
    /// <param name="color">the color to clear</param>
    void ClearColor(int color) {
        // Loop through all tiles, if the tile is part of a blob of adequate size and matching color, clear that tile
        for (int y = 0; y < board.manaTileGrid.height; y++) {
            for (int x = 0; x < board.manaTileGrid.width; x++) {
                List<Vector2Int> blob = blobGrid[x, y];
                if (blob != null && blob.Count >= minBlobSize) {
                    Vector2Int position = new Vector2Int(x,y);
                    ManaTile tile = board.manaTileGrid.GetTile(position);
                    if (tile.color == color) {
                        board.manaTileGrid.ClearTile(position);
                    }
                }
            }
        }
    }
}