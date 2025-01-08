using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages the Battle and the CharSelect scenes, 
/// and ensures proper coordination bewteen the two during local and online matches that switch between the two scenes.
/// </summary>
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// NetworkManager component that sits on the same object as this GameManager. Cached on awake.
    /// Used to start host/client.
    /// In online, will start as the host and other player connect as clients.
    /// In local multiplayer and singleplayer, only this client connects, but network manager is still needed for networkvariables.
    /// Could make separate online and offline classes but nah.
    /// </summary>
    private NetworkManager networkManager;

    /// <summary>
    /// Component on this gameobject that controls the spawning and despawning of players on the network.
    /// Only used when hosting a game. Cannot be used on non-server clients.
    /// </summary>
    private ServerPlayerManager serverPlayerManager;

    /// <summary>
    /// (Server/Host only) Current player connection manager being used.
    /// Different playermanagers are used for different gamemodes!
    /// When a gamemode is loaded the corresponding playermanager will be instantiated.
    /// When 
    /// </summary>
    private IServerPlayerConnectionManager serverPlayerConnectionManager;

    /// <summary>
    /// Battle data to use for the current game, such as the RNG seed, cycle length and color count, etc
    /// </summary>
    public BattleData battleData;

    public enum GameConnectionType {
        /// <summary>
        /// Means that no game is initialized right now, meaning player is in main menu or some other non-lobby scene
        /// </summary>
        None, 

        /// <summary>
        /// Multiple clients control multiple players
        /// </summary>
        OnlineMultiplayer,

        /// <summary>
        /// Single client, adds a player for each local device connected
        /// </summary>
        LocalMultiplayer,

        /// <summary>
        /// Single client, only one client-controlled player
        /// </summary>
        Singleplayer
    }
    public GameConnectionType gameConnectionType {get; private set;} = GameConnectionType.None;


    public enum GameState {
        Menu,
        Playing,
        Paused,
        PostGame
    }
    public GameState gameState {get; private set;}


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Duplicate GameManager spawned, destroying the newly instantiated one");
            Destroy(gameObject);
        }

        networkManager = GetComponent<NetworkManager>();
    }

    /// <summary>
    /// Used to start a new game, or join an existing game.
    /// Should be called after the network manager starts.
    /// </summary>
    /// <param name="gameConnectionType">The type of connection used to connect with other players</param>
    /// <param name="host">Start host if true, start client if false (should only be false if connecting to another player in online mode)</param>
    public void StartGameHost(GameConnectionType gameConnectionType) {
        // TODO: pass any other per-game settings needed here, such as the solo mode level

        // Can't starta game if a game is already active
        if (IsGameActive()) {
            Debug.LogError("Trying to start a new game, but a game is already initialized!");
            return;
        }

        // Network manager needed for GameManager to work properly
        if (!NetworkManager.Singleton) {
            Debug.LogError("Trying to start a game, but there is no NetworkManager present!");
            return;
        }

        if (!NetworkManager.Singleton.IsServer) {
            Debug.LogError("Trying to start a hosted game, but the network manager is not running as a server!");
            return;
        }

        // Start the approprite player connection manager based on the connection type of the game
        if (gameConnectionType == GameConnectionType.OnlineMultiplayer) {
            serverPlayerConnectionManager = new OnlinePlayerConnectionManager();
        }
        else if (gameConnectionType == GameConnectionType.LocalMultiplayer) {
            serverPlayerConnectionManager = new LocalPlayerConnectionManager();
        }
        else {
            serverPlayerConnectionManager = new SingleplayerConnectionManager();
        }

        Debug.Log("Started a game as a host with connection type "+gameConnectionType);

        this.gameConnectionType = gameConnectionType;
    }

    /// <summary>
    /// Stop the current game. Used when a game is no longer needed (going from battle/charselect to main menu or other non-game scene).
    /// Current game must be stopped before another game can be started.
    /// </summary>
    public void StopGame() {
        if (gameConnectionType == GameConnectionType.None) {
            Debug.LogError("Trying to stop the current game but there is no game initialized!");
            return;
        }
    }

    public void GameStopped() {
        gameConnectionType = GameConnectionType.None;
    }

    /// <summary>
    /// Returns true if game type is not the default value, meaning a game is already started
    /// </summary>
    public bool IsGameActive() {
        return gameConnectionType != GameConnectionType.None;
    }
}