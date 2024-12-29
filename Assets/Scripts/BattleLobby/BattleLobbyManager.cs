using Unity.Netcode;
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
    /// Instance of the PlayerManager in the scenes
    /// </summary>
    public PlayerManager playerManager;

    /// <summary>
    /// Instance of the NetworkManager in the scenes
    /// </summary>
    public NetworkManager networkManager;

    /// <summary>
    /// Battle player input manager prefab to spawn in battle or battlesetup scenes (singleplayer / local multiplayer only)
    /// </summary>
    public BattlePlayerInputManager battlePlayerInputManagerPrefab;

    /// <summary>
    /// Instance of the BattlePlayerInputManager in the scene (only used in singleplayer/local multiplayer)
    /// </summary>
    public BattlePlayerInputManager battlePlayerInputManager;

    /// <summary>
    /// The battle setup manager in the battle setup scene. Value set by battlesetupmanager on awake. Null while in battle
    /// </summary>
    public BattleSetupManager battleSetupManager;

    /// <summary>
    /// The battle manager in the battle scene. Value set by battlemanager on awake. Null while in battle setup
    /// </summary>
    public BattleManager battleManager;

    public enum BattleType {
        NONE, // default value, make sure to change to a vlaue other than this
        SINGLEPLAYER, // deny all join requests, single host in "multiplayer" mode
        LOCAL_MULTIPLAYER, // players can join on host client via devices, deny other network clients from joining

        ONLINE_MULTIPLAYER, // other clients can join via session
    }

    /// <summary>
    /// The type of battle currently being setup/played.
    /// </summary>
    public BattleType battleType = BattleType.NONE;

    /// <summary>
    /// Current session this client is connected to if in online mode.
    /// </summary>
    public ISession current_session;

    public enum BattlePhase {
        BATTLE_SETUP,
        BATTLE
    }
    /// <summary>
    /// The current phase of the battle, CHARACTER_SELECT or BATTLE, depending on the last scene that was loaded.
    /// </summary>
    public BattlePhase battlePhase;

    /// <summary>
    /// Start the host on the networkmanager if not already started.
    // Mainly used for testing when directly loading into battle scene without starting host in battle setup scene
    /// </summary>
    public void StartNetworkManagerHost() {
        if (!networkManager) {
            Debug.LogError("Battle network scene is not loaded, can't start host!");
            return;
        }

        if (!networkManager.IsHost && !networkManager.IsClient) {
            networkManager.StartHost();
        } else {
            Debug.Log("Host already started");
        }
    }
}