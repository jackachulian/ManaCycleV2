using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Multiplayer.Widgets;
using Unity.Services.Multiplayer;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class CharacterSelectMenu : MonoBehaviour
{
    /// <summary>
    /// All four battle setup player panels in the character select menu.
    /// </summary>
    [SerializeField] private BattleSetupPlayerPanel[] playerPanels;

    /// <summary>
    /// Shows the join code at the bottom of the char select screen during online mode.
    /// </summary>
    [SerializeField] private ShowJoinCode showJoinCode;

    /// <summary>
    /// Initializes the char select. Should only be called from the battle setup manager
    /// </summary>
    public void InitializeBattleSetup() {
        // show the menu
        gameObject.SetActive(true);

        // show the session code (Online mode only)
        if (BattleLobbyManager.battleType == BattleLobbyManager.BattleType.ONLINE_MULTIPLAYER) {
            if (BattleLobbyManager.current_session != null) {
                Debug.LogError("No session found while in online mode in character select!");
            } else {
                showJoinCode.Session = BattleLobbyManager.current_session;
                showJoinCode.OnSessionJoined(); 
            }
        }

        // initialize battle panels
        foreach (BattleSetupPlayerPanel playerPanel in playerPanels) {
            playerPanel.InitializeBattleSetup(this);
            playerPanel.ready.OnValueChanged += CheckIfAllPlayersReady;
            // spawn the player panels on the network if this is the server
            if (BattleNetworkManager.instance.IsServer) playerPanel.GetComponent<NetworkObject>().Spawn();
        }

        

        // load the local player inputs needed for local play.
        // don't do this in online mode
        if (BattleLobbyManager.battleType != BattleLobbyManager.BattleType.ONLINE_MULTIPLAYER) {
            if (!PlayerInputManager.instance) {
                SceneManager.LoadScene("LocalPlayerManagement", LoadSceneMode.Additive);
                Debug.Log("loaded local player management scene");
            } else {
                Debug.Log("Not loading player input manager because an instance already exists");
            }
        }

        
        
    }

    public void CheckIfAllPlayersReady(bool previous, bool current) {
        // Only the server/host can start the game
        if (!BattleNetworkManager.instance.IsServer) return;

        // Make sure all players are ready
        foreach (BattleSetupPlayerPanel playerPanel in playerPanels) {
            if (!playerPanel.ready.Value) {
                return;
            }
        }

        // The server/host chooses the seed that will be used for piece RNG.
        BattleSettings settings = new BattleSettings();
        settings.seed = Random.Range(0, int.MaxValue);

        //TODO: transition / battle start sound/animation
        StartGameRpc(settings);
    }

    [Rpc(SendTo.Everyone)]
    public void StartGameRpc(BattleSettings settings) {
        BattleManager.Configure(settings);
        SceneManager.LoadScene("Battle");
    }
}
