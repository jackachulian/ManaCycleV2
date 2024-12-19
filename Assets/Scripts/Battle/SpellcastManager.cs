using UnityEngine;

/// <summary>
/// A component to be used on the same gameobject as the Board.
/// Handles spellcasting and the timing associated with it.
/// </summary>
public class SpellcastManager : MonoBehaviour {
    /// <summary>
    /// The Board this is managing the spellcasts of. Cached on InitializeBattle()
    /// </summary>
    private Board board;

    /// <summary>
    /// Called when the battle initializes, after the Board for this spellcastmanager is fully initialized.
    /// </summary>
    /// <param name="board"></param>
    public void InitializeBattle(Board board) {
        this.board = board;
    }

    /// <summary>
    /// Should be set to true whenever the board's grid is modified.
    /// Lets this script know if it needs to re-check for blob connections
    /// </summary>
    private bool needsUpdate;



    public void FindConnectedTiles() {

    }
}