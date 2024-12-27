using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages continuity between battle and battle setup when moving between selecting characters and battling in local/online.
/// Static utility scriptable Object class, but holds references to MonoBehaviours that are in the current scenes.
/// </summary>
[CreateAssetMenu(fileName = "BattleLobbyConfig", menuName = "ManaCycle/BattleLobbyManager", order = 1)]
public class BattleLobbyManager : ScriptableObject {
    /// <summary>
    /// Battle network manager prefab to spawn in battle or battlesetup scenes
    /// </summary>
    public BattleNetworkManager battleNetworkManagerPrefab;

    /// <summary>
    /// Instance of the BattleNetworkManager in the scene
    /// </summary>
    public BattleNetworkManager battleNetworkManager {get; set;}

    /// <summary>
    /// The battle setup manager in the battle setup scene. Value set by battlesetupmanager on awake. Null while in battle
    /// </summary>
    public BattleSetupManager battleSetupManager {get; set;}

    /// <summary>
    /// The battle manager in the battle scene. Value set by battlemanager on awake. Null while in battle setup
    /// </summary>
    public BattleManager battleManager {get; set;}

    public enum BattleType {
        NONE, // default value, make sure to change to a vlaue other than this
        SINGLEPLAYER, // deny all join requests, single host in "multiplayer" mode
        LOCAL_MULTIPLAYER, // players can join on host client via devices, deny other network clients from joining

        ONLINE_MULTIPLAYER, // other clients can join via session
    }

    /// <summary>
    /// The type of battle currently being setup/played.
    /// </summary>
    public BattleType battleType {get; set;} = BattleType.NONE;

    /// <summary>
    /// Current session this client is connected to if in online mode.
    /// </summary>
    public ISession current_session {get; set;}

    public enum BattlePhase {
        BATTLE_SETUP,
        BATTLE
    }
    /// <summary>
    /// The current phase of the battle, CHARACTER_SELECT or BATTLE, depending on the last scene that was loaded.
    /// </summary>
    public BattlePhase battlePhase {get; set;}

    /// <summary>
    /// Ensures that the network manager scene is loaded additively. Won't load it if it's already loaded.
    /// </summary>
    public void StartNetworkManagerScene() {
        if (battleNetworkManager == null) {
            Debug.Log("Instantiating network manager");
            battleNetworkManager = Instantiate(battleNetworkManagerPrefab);
        } else {
            Debug.Log("Skipping network management scene load, battle network manager already present");
        }
    }

    /// <summary>
    /// Start the host on the networkmanager if not already started.
    // Mainly used for testing when directly loading into battle scene without starting host in battle setup scene
    /// </summary>
    public void StartNetworkManagerHost() {
        if (!battleNetworkManager) {
            Debug.LogError("Battle network scene is not loaded, can't start host!");
            return;
        }

        if (!battleNetworkManager.IsHost) {
            battleNetworkManager.StartHost();
        } else {
            Debug.Log("Host already started");
        }
    }
}