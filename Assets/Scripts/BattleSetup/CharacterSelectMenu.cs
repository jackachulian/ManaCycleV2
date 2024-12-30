using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Multiplayer.Widgets;
using Unity.Services.Multiplayer;
using UnityEngine.InputSystem;
using Unity.Netcode;

/// <summary>
/// The character select menu and all of its UI-related components and functionalities.
/// For network-related CharacterSelect RPCs and server functions go to the CharacterSelectNetworkBehaviour.
/// </summary>
public class CharacterSelectMenu : MonoBehaviour
{
    /// <summary>
    /// Stores shared battle lobby dependencies
    /// </summary>
    [SerializeField] private BattleLobbyManager battleLobbyManager;

    /// <summary>
    /// All four battle setup player panels in the character select menu.
    /// </summary>
    [SerializeField] private BattleSetupPlayerPanel[] playerPanels;

    /// <summary>
    /// Shows the join code at the bottom of the char select screen during online mode.
    /// </summary>
    [SerializeField] private ShowJoinCode showJoinCode;

    /// <summary>
    /// Controls network-related RPCs, seperate from the UI.
    /// </summary>
    [SerializeField] private CharacterSelectNetworkBehaviour _characterSelectNetworkBehaviour;

    /// <summary>
    /// Controls network-related RPCs, separate from the UI. (Public accessor)
    /// </summary>
    public CharacterSelectNetworkBehaviour characterSelectNetworkBehaviour => _characterSelectNetworkBehaviour;
    
    /// <summary>
    /// Initializes the char select. Should only be called from the battle setup manager
    /// </summary>
    public void InitializeBattleSetup() {
        // show the menu
        gameObject.SetActive(true);

        // show the session code (Online mode only)
        if (battleLobbyManager.battleType == BattleLobbyManager.BattleType.ONLINE_MULTIPLAYER) {
            showJoinCode.gameObject.SetActive(true);
            if (battleLobbyManager.current_session == null) {
                Debug.LogError("No session found while in online mode in character select!");
            } else {
                showJoinCode.Session = battleLobbyManager.current_session;
                showJoinCode.OnSessionJoined(); 
            }
        } else {
            showJoinCode.gameObject.SetActive(false);
        }

        // initialize battle panels
        foreach (BattleSetupPlayerPanel playerPanel in playerPanels) {
            playerPanel.InitializeBattleSetup(this);
        }        
    }

    public void HideUI() {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Spawn the network behaviour that will control this UI.
    /// Only works if this client is the host.
    /// </summary>
    public void SpawnNetworkObject() {
        Debug.Log("Spawning character select network behaviour....");
        characterSelectNetworkBehaviour.GetComponent<NetworkObject>().Spawn();
    }

    /// <summary>
    /// Get the setuppanel with the given index.
    /// The controlled panel will have the same index as the board the player will control in battle.
    /// </summary>
    public BattleSetupPlayerPanel GetPanel(int index) {
        return playerPanels[index];
    }
}
