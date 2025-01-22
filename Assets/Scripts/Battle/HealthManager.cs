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
    /// Counter incoming damage first, and then deal damage to all other players via a network RPC if any is leftover.
    /// </summary>
    public void DealDamageToAllOtherBoards(int damage) {
        damage = board.healthManager.CounterIncomingDamage(damage);
        if (damage <= 0) return;

        int playerCount = GameManager.Instance.playerManager.players.Count;
        int otherLivingBoardCount = 0;
        for (int i = 0; i < playerCount; i++) {
            Board otherBoard = BattleManager.Instance.GetBoardByIndex(i);
            if (otherBoard != board && otherBoard.boardActive) {
                // todo: when teams mode is added, only track/deal damage to board if different team
                otherLivingBoardCount += 1;
            }
        }

        int damagePerBoard = damage / Mathf.Max(otherLivingBoardCount, 1);
        for (int i = 0; i < playerCount; i++) {
            Board otherBoard = BattleManager.Instance.GetBoardByIndex(i);
            if (otherBoard != board && otherBoard.boardActive) {
                // Send to the other board for them to enqueue themselves so it is synchronized and order-guaranteed 
                // with their health changing events such as spellcasting.
                otherBoard.player.boardNetworkBehaviour.EnqueueDamageRpc(damagePerBoard);
            }
        }
    }

    /// <summary>
    /// Counter incoming damage - closest to end of list first.
    /// Return the amount of leftover damage after countering, if any.
    /// </summary>
    public int CounterIncomingDamage(int damage) {
        for (int i = 5; i >= 1; i--) {
            if (incomingDamage[i] >= damage) {
                incomingDamage[i] -= damage;
                return 0;
            } else {
                damage -= incomingDamage[i];
                incomingDamage[i] = 0;
            }
        }

        return damage;
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
            health -= incomingDamage[5];
            board.ui.OnDamage();
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