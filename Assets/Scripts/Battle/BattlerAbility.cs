using UnityEngine;

public abstract class BattlerAbility : MonoBehaviour {
    /// <summary>
    /// Amount of SP required to use this battler's active ability
    /// </summary>
    [SerializeField] private int _activeAbilityCost = 40;
    public int activeAbilityCost => _activeAbilityCost;

    /// <summary>
    /// Called when a player with this ability is connected to a board.
    /// Sets up passive abilities and other things that need to be initialized per board
    /// </summary>
    public abstract void OnPlayerBoardConnected();

    /// <summary>
    /// Call this when a player is disconnected from a board, typically when the battle ends.
    /// </summary>
    public abstract void OnPlayerBoardDisconnected();

    /// <summary>
    /// Called when a player uses their ability
    /// </summary>
    public abstract void UseAbility();
}