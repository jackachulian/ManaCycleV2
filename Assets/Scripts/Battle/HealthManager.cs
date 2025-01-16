using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles dealing and countering/receiving damage. 
/// Sync'd over the network to have the same order of events as piece placing and spellcasting.
/// (Is on the same NetworkObject as PieceManager and SpellcastManager, so same RPC execution order is guaranteed.)
/// </summary>
public class HealthManager : MonoBehaviour {
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
    [SerializeField] private IncomingDamageUI incomingDamageUI;


    private Board board;

    /// <summary>
    /// Called when the battle initializes, after the ManaCycle and the Board for this healthmanager is initialized.
    /// </summary>
    /// <param name="board"></param>
    public void InitializeBattle(Board board) {
        this.board = board;
        incomingDamage = new int[6];
        UpdateHealthUI();
    }

    public void UpdateHealthUI() {
        hpBarUI.UpdateUI(health, maxHealth, incomingDamage);
        incomingDamageUI.UpdateUI(incomingDamage);
    }


    /// <summary>
    /// (Rpc Target) Add damage to the start of the damage queue.
    /// </summary>
    public void EnqueueDamage(int damage) {
        // TODO: implement the incoming damage bar
        incomingDamage[0] += damage;

        UpdateHealthUI();
    }

    public void AdvanceDamageQueue() {
        if (incomingDamage[5] > 0) {
            // TODO: damage animation
            health -= incomingDamage[5];
            board.boardUI.OnDamage();
        }

        for (int i = 5; i >= 1; i--) {
            incomingDamage[i] = incomingDamage[i - 1];
        }

        incomingDamage[0] = 0;

        if (health <= 0) {
            board.Defeat();
        }

        UpdateHealthUI();
    }
}