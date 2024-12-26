using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Multiplayer.Widgets;
using Unity.Services.Multiplayer;
using UnityEngine.InputSystem;

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

    private void OnEnable() {
        foreach (BattleSetupPlayerPanel playerPanel in playerPanels) {
            playerPanel.InitializeBattleSetup(this);
            playerPanel.onReadyChanged.AddListener(CheckIfAllPlayersReady);
        }

        if (BattleSetupManager.instance && BattleSetupManager.instance.current_session != null) {
            showJoinCode.Session = BattleSetupManager.instance.current_session;
            showJoinCode.OnSessionJoined(); // will show the session's code
        }
    }

    private void OnDisable() {
        foreach (BattleSetupPlayerPanel playerPanel in playerPanels) {
            playerPanel.onReadyChanged.RemoveListener(CheckIfAllPlayersReady);
        }
    }

    /// <summary>
    /// Initializes the char select. Should only be called from the battle setup manager
    /// </summary>
    public void InitializeBattleSetup() {
        // show the menu
        gameObject.SetActive(true);

        // load the local player inputs needed for local play.
        // TODO: don't do this in online mode
        if (!BattleSetupManager.online) {
            if (!PlayerInputManager.instance) {
                SceneManager.LoadScene("LocalPlayerManagement", LoadSceneMode.Additive);
                Debug.Log("loaded local player management scene");
            } else {
                Debug.Log("Not loading player input manager because an instance already exists");
            }
        }
    }

    public void CheckIfAllPlayersReady() {
        foreach (BattleSetupPlayerPanel playerPanel in playerPanels) {
            if (!playerPanel.ready) {
                return;
            }
        }

        // if code reaches here, all players are ready, start game on next start press from a player.
        //TODO: transition / battle start sound/animation
        SceneManager.LoadScene("Battle");
    }
}