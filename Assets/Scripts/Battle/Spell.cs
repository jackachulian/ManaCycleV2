using UnityEngine;

public abstract class Spell : MonoBehaviour {
    /// <summary>
    /// Amount of SP required to use this battler's active ability
    /// </summary>
    [SerializeField] private int _spCost = 40;
    public int spCost => _spCost;


    public Board board {get; private set;}

    /// <summary>
    /// Called when a player with this ability is connected to a board.
    /// Sets up passive abilities and other things that need to be initialized per board
    /// </summary>
    public virtual void OnPlayerBoardConnected(Board board) {
        this.board = board;
    }

    /// <summary>
    /// Call this when a player is disconnected from a board, typically when the battle ends.
    /// </summary>
    public virtual void OnPlayerBoardDisconnected(Board board) {
        this.board = null;
    }

    /// <summary>
    /// Called when a player uses their ability
    /// </summary>
    public abstract void Use(Board board);
}