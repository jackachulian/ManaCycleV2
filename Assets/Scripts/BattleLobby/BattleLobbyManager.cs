using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages continuity between battle and battle setup when moving between selecting characters and battling in local/online.
/// Static utility non-MonoBehaviour class, but holds references to MonoBehaviours in the current scenes.
/// </summary>
public class BattleLobbyManager {
    /// <summary>
    /// The battle setup manager in the battle setup scene. Null while in battle
    /// </summary>
    public static BattleSetupManager battleSetupManager;

    /// <summary>
    /// The battle manager in the battle scene. Null while in battle setup
    /// </summary>
    public static BattleManager battleManager;

    public enum BattleType {
        NONE, // default value, make sure to change to a vlaue other than this
        SINGLEPLAYER, // deny all join requests, single host in "multiplayer" mode
        LOCAL_MULTIPLAYER, // players can join on host client via devices, deny other network clients from joining

        ONLINE_MULTIPLAYER, // other clients can join via session
    }

    /// <summary>
    /// The type of battle currently being setup/played.
    /// </summary>
    public static BattleType battleType = BattleType.NONE;

    /// <summary>
    /// Current session this client is connected to if in online mode.
    /// </summary>
    public static ISession current_session {get; set;}

    public enum BattlePhase {
        BATTLE_SETUP,
        BATTLE
    }
    /// <summary>
    /// The current phase of the battle, CHARACTER_SELECT or BATTLE, depending on the last scene that was loaded.
    /// </summary>
    public static BattlePhase battlePhase;



    /// <summary>
    /// Ensures that the network manager scene is loaded additively. Won't load it if it's already loaded.
    /// </summary>
    public static void StartNetworkManagerScene() {
        if (BattleNetworkManager.instance == null) {
            Debug.Log("Starting network management scene");
            SceneManager.LoadScene("NetworkManagement", LoadSceneMode.Additive);
        } else {
            Debug.Log("Skipping network management scene load, battle network manager already present");
        }
    }

    /// <summary>
    /// Start the host on the networkmanager if not already started.
    // Mainly used for testing when directly loading into battle scene without starting host in battle setup scene
    /// </summary>
    public static void StartNetworkManagerHost() {
        if (!BattleNetworkManager.instance) {
            Debug.LogError("Battle network scene is not loaded, can't start host!");
            return;
        }

        if (!BattleNetworkManager.instance.IsHost) {
            BattleNetworkManager.instance.StartHost();
        } else {
            Debug.Log("Host already started");
        }
    }
}