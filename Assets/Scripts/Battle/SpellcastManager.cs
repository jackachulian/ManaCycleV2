using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Audio;

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
    private int[] clearableManaCounts;

    /// <summary>
    /// Minimum blob size required to clear
    /// </summary>
    public readonly int minBlobSize = 3;

    // ======== Spellcasting ========
    /// <summary>
    /// Whether or not a spellcast is currently in progress.
    /// </summary>
    private bool spellcasting {get; set;}

    /// <summary>
    /// Current cascade off the current color during a spellcast.
    /// Set to 0 when a spellcast is not occuring.
    /// If after a clear, the same color is clearable again, this is set to 1.
    /// If still clearable after cascadeDelay, trigger a cascade clear and increment by 1.
    /// Once cascade ends set back to 0.
    /// </summary>
    private int currentCascade;

    /// <summary>
    /// Current chain during a spellcast
    /// </summary>
    private int currentChain;

    /// <summary>
    /// Is set to 0 when a clear happens during a spellcast and counts upward.
    /// Used for spellcast cascade/chain timing.
    /// This value is only managed on the client controlling this board, 
    /// so don't use this value within RPCs that are sent to all clients!
    /// </summary>
    private float timeSinceLastClear;

    /// <summary>
    ///  Event raised when this board's position in the cycle is changed
    /// </summary>
    public event Action<int> onCycleIndexChanged; 
    
    /// <summary>
    /// Called when the battle initializes, after the ManaCycle and the Board for this spellcastmanager is initialized.
    /// </summary>
    /// <param name="board"></param>
    public void InitializeBattle(Board board) {
        this.board = board;
        blobGrid = new List<Vector2Int>[board.manaTileGrid.width, board.manaTileGrid.height];
        clearableManaCounts = new int[GameManager.Instance.battleData.cycleUniqueColors];

        board.ui.RepositionCyclePointer(cycleIndex);

        // Cascade popup will not use the time display (for now at least, may change this behaviour later)
        board.ui.cascadePopup.DisplayTimeLeft(0);
    }

    void Update() {
        if (!board || !board.boardActive) return;

        if (spellcasting)
        {
            SpellcastTimingUpdate();
            SpellcastMaterialUpdate();
        }
    }

    /// <summary>
    /// Updates the state of the spellcast based on several timing variables and the changing state of the board.
    /// Runs each frame. (Only on the owner of this board!)
    /// </summary>
    void SpellcastTimingUpdate() {
        // only manage spellcast timing while a spellcast is active
        if (!spellcasting) return;

        // Only perform spellcast timing logic if this board is owned
        if (!board.player || !board.player.IsOwner) return;

        timeSinceLastClear += Time.deltaTime;

        // Display the time remaining to extend the chain
        board.ui.chainPopup.DisplayTimeLeft((maxChainDelay - timeSinceLastClear) / maxChainDelay);

        // Check to see if there is a valid cascade
        if (
            currentCascade > 0   // current cascade must be 1 or higher, meaning that a cascade was possible the instant after the last clear happened.
            && timeSinceLastClear >= cascadeDelay   // must happen after cascade delay
            && timeSinceLastClear < chainDelay      // must happen before chain delay
            && IsCurrentColorClearable()            // current color in cycle must be clearable
        ) {
            board.player.boardNetworkBehaviour.SpellcastClearRpc();
            timeSinceLastClear = 0;
        } 
        // If a cascade is ongoing, but there is not a valid cascade and cascade period has passed (chain delay reached), 
        // end the cascade and move to the next color in the chain.
        // note: this will only be reached if a cascade is set to happen right after a clear, but an ability tile breaks the cascade before cascadeDelay passes
        else if (currentCascade > 0 && timeSinceLastClear >= chainDelay) {
            board.player.boardNetworkBehaviour.EndCascadeRpc();
        }

        // Check to see if there is a valid chain
        if (
            timeSinceLastClear >= chainDelay    // must happen after chain delay
            && timeSinceLastClear < maxChainDelay   // must happen before max chain delay
            && IsCurrentColorClearable()            // current color in cycle must be clearable
        ) {
            board.player.boardNetworkBehaviour.SpellcastClearRpc();
            timeSinceLastClear = 0;
        } 

        // otherwise, if past max chain delay, end the chain here
        else if (timeSinceLastClear >= maxChainDelay) {
            board.player.boardNetworkBehaviour.EndChainRpc();
        }
    }

    void SpellcastMaterialUpdate()
    {
        for (int i = 0; i < board.fadeGlowMaterials.Length; i++)
        {
            Material fadeGlowMaterial = board.fadeGlowMaterials[i];
            float litAmount = Mathf.Clamp01(Mathf.InverseLerp(0, chainDelay, timeSinceLastClear));
            fadeGlowMaterial.SetFloat("_LitAmount", litAmount);
        }
    }

    public void EndChain() {
        spellcasting = false;
        currentChain = 0;
        currentCascade = 0;
        board.ui.chainPopup.Hide();
        board.ui.cascadePopup.Hide();
        SpellcastMaterialUpdate();
        UpdateFadeGlow();
        Debug.Log("Chain ended");
    }

    
    /// <summary>
    /// (Rpc Target) Clear the current color, add to chain/cascade, and score appropriate amount of points
    /// </summary>
    public void SpellcastClear() {
        // if a cascade is happening, add to cascade, otherwise add to chain
        if (currentCascade > 0) {
            Debug.Log("Cascade clearing - chain="+currentChain+", cascade="+currentCascade+", color="+GetCurrentCycleColor());
            currentCascade += 1;
            AudioManager.Instance.PlayBoardSound("cascade", pitch: 1f + Math.Min(currentChain + currentCascade, 12) * 0.12f);
        } else {
            Debug.Log("Chain clearing - chain="+currentChain+", cascade="+currentCascade+", color="+GetCurrentCycleColor());
            currentChain += 1;
            AudioManager.Instance.PlayBoardSound("cast", pitch: 1f + Math.Min(currentChain + currentCascade, 12) * 0.12f);
        }

        // Show the chain popup if chain is 2 or greater
        if (currentChain >= 2) {
            board.ui.chainPopup.Show(currentChain);
        }

        // Show the chain popup if cascade is 2 or greater
        if (currentCascade >= 2) {
            board.ui.cascadePopup.Show(currentCascade);
        }

        // Clear the current cycle color and recalculate connected tiles
        double totalMana = ClearColor(GetCurrentCycleColor());
        double damagePerMana = 5f;
        double chainMultiplier = currentChain;

        // if no cascade is happening (cascade == 0), no cascade bonus
        // if a cascade IS happening but has not activated yet: 1 -> 1.0, 2 -> 2.0, 3 -> 4.0, 4 -> 8.0, etc
        double cascadeMultiplier = currentCascade > 0 ? Math.Pow(currentCascade - 1, 2.0) : 1.0;

        // Main damage formula
        int damage = (int)(totalMana * damagePerMana * chainMultiplier * cascadeMultiplier);

        Debug.Log("total mana: " + totalMana + ", chain: " + currentChain + ", cascade: " + currentCascade + ", damage: " + damage + " (" + totalMana + "*" + damagePerMana + "*" + chainMultiplier + "*" + cascadeMultiplier + ")");

        // Counter incoming  (Method will return the amount of leftover damage after countering)
        // evaluate this on both clients here since it should happen at the same time as spellcast clear
        damage = board.healthManager.CounterIncomingDamage(damage);
        board.healthManager.UpdateHealthUI();

        // TODO: factor in how many boards are actually currently controlled, and split damage equally among them
        // for now just deal damage to all other boards.
        // (client decides how much damage it takes)

        // another possible security improvement: 
        // have each client store the expected and actual damage of a damage instance to determine if the other client is cheating
        if (board.player && board.player.IsOwner) {
            board.healthManager.DealDamageToAllOtherBoards(damage);
        } else {
            // check actual (received) value if present, wait for it if not
            // and then compare it against expected damage (the local damage variable)
        }
        

        board.manaTileGrid.AllTileGravity();
        RefreshBlobs();
        board.ghostPieceManager.UpdateGhostPiece();

        // If the next color is immediately clearable, start a cascade if not already cascading, or leave the current cascade continuing
        if (IsCurrentColorClearable()) {
            if (currentCascade == 0) currentCascade = 1;
        }
        // otherwise, advance to the next color in the cycle
        else {
            AdvanceCycle();
        }

        UpdateFadeGlow();
    }

    /// <summary>
    /// (Rpc Target) End the current cascade and advance to the next color in the sequence.
    /// </summary>
    public void EndCascade() {
        currentCascade = 0;
        board.ui.cascadePopup.Hide();
        AdvanceCycle();
        UpdateFadeGlow();
    }

    /// <summary>
    /// Move this board's pointer to the next sequential color in the cycle.
    /// </summary>
    private void AdvanceCycle() {
        cycleIndex += 1;
        currentCascade = 0;
        if (cycleIndex >= GameManager.Instance.battleData.cycleLength) {
            cycleIndex = 0;
            // TODO: cycle multiplier
        }

        board.ui.RepositionCyclePointer(cycleIndex);
        onCycleIndexChanged?.Invoke(cycleIndex);
        // TODO: Maybe implement all board ui functions using c# events
        board.ui.OnSpellcast();
    }

    /// <summary>
    /// Try to perform a spellcast.
    /// </summary>
    public void TrySpellcast() {
        if (!board.boardActive || spellcasting) return;

        if (!IsCurrentColorClearable()) {
            Debug.Log("Can't spellcast!");
            board.ui.OnFailSpellcast(GetCurrentCycleColor());
            AudioManager.Instance.PlayBoardSound("cast_fail");
            return;
        }

        board.player.boardNetworkBehaviour.StartSpellcastRpc();
    }

    /// <summary>
    /// (Rpc Target) Called when a player begins a spellcast on their client.
    /// </summary>
    public void StartSpellcast() {
        spellcasting = true;
        timeSinceLastClear = 0;
        currentCascade = 0;
        currentChain = 0;
        AudioManager.Instance.PlayBoardSound("cast_startup");
        Debug.Log("Spellcast has begun!");
        UpdateFadeGlow();
    }

    /// <summary>
    /// Apply fade-glow to all tiles that are within any adequately-sized blob of the current color.
    /// </summary>
    public void UpdateFadeGlow()
    {
        for (int y = 0; y < board.manaTileGrid.height; y++)
        {
            for (int x = 0; x < board.manaTileGrid.width; x++)
            {
                ManaTile tile = board.manaTileGrid.GetTile(new Vector2Int(x, y));
                if (tile)
                {
                    // only fade if in a blob while spellcasting
                    var blob = blobGrid[x, y];
                    tile.SetFadeGlow(spellcasting && blob != null && blob.Count >= minBlobSize && tile.color == GetCurrentCycleColor());
                    tile.UpdateVisuals(board: board);
                }
            }
        }
    }

    /// <summary>
    /// Returns the color that this board is currently on and has to clear in order to advance the cycle.
    /// </summary>
    /// <returns>the int representing the current color</returns>
    public int GetCurrentCycleColor() {
        return board.GetManaCycle().GetSequenceColor(cycleIndex);
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
        // Reset the blob array to empty
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

        UpdateFadeGlow();
    }

    /// <summary>
    /// Recursively expands the passed blob to all connected tiles
    /// </summary>
    /// <param name="blob">the blob (list of vecor2ints) to add to</param>
    /// <param name="position">position of the possible tile to try to add to the blob</param>
    /// <param name="color">the color to check for </param>
    private void ExpandBlob(List<Vector2Int> blob, Vector2Int position, int color)
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
    /// <returns>the total amount of mana cleared 
    /// (as a float, as some pieces have increased mana multipliers and count as more than 1 mana)</returns>
    private float ClearColor(int color) {
        // Loop through all tiles, if the tile is part of a blob of adequate size and matching color, clear that tile
        float manaTotal = 0f;
        for (int y = 0; y < board.manaTileGrid.height; y++) {
            for (int x = 0; x < board.manaTileGrid.width; x++) {
                List<Vector2Int> blob = blobGrid[x, y];
                if (blob != null && blob.Count >= minBlobSize) {
                    Vector2Int position = new Vector2Int(x,y);
                    ManaTile tile = board.manaTileGrid.GetTile(position);
                    if (tile.color == color) {
                        board.manaTileGrid.ClearTile(position);
                        manaTotal += 1;
                    }
                }
            }
        }
        return manaTotal;
    }
}