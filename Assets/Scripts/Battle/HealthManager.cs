using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles dealing and countering/receiving damage. 
/// Sync'd over the network to have the same order of events as piece placing and spellcasting.
/// (Is on the same NetworkObject as PieceManager and SpellcastManager, so same RPC execution order is guaranteed.)
/// </summary>
public class HealthManager : NetworkBehaviour {
    /// <summary>
    /// Current health. When this reaches 0, player is defeated.
    /// </summary>
    public int health {get; private set;} = 1000;

    /// <summary>
    /// Total maximum health. Used to find HP percentage to display on hp bar shader. Cannot heal above this value.
    /// </summary>
    public int maxHealth {get; private set;} = 1000;

    /// <summary>
    /// List of incoming damage. Each time a piece is placed, each number moves down the list. When a number reaches the end,
    /// this player takes that amount of damage.
    /// </summary>
    public int[] incomingDamage {get; private set;}

    [SerializeField] private HpBarUI hpBarUI;

    /// <summary>
    /// Called when the battle initializes, after the ManaCycle and the Board for this healthmanager is initialized.
    /// </summary>
    /// <param name="board"></param>
    public void InitializeBattle(Board board) {
        incomingDamage = new int[6];
        UpdateHpBarVisual();
    }

    public void UpdateHpBarVisual() {
        hpBarUI.UpdateHpVisual(health, maxHealth, incomingDamage);
    }

    /// <summary>
    /// Receive damage from another board.
    /// </summary>
    /// <param name="damage">the amount of damage dealt by the other player</param>
    [Rpc(SendTo.Everyone)]
    public void EnqueueDamageRpc(int damage) {
        // TODO: implement the incoming damage bar
        incomingDamage[0] += damage;

        UpdateHpBarVisual();
    }

    public void AdvanceDamageQueue() {
        if (incomingDamage[5] > 0) {
            // TODO: damage animation
            health -= incomingDamage[5];
        }

        for (int i = 5; i >= 1; i--) {
            incomingDamage[i] = incomingDamage[i - 1];
        }

        incomingDamage[0] = 0;

        UpdateHpBarVisual();
    }
}