using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the Battle and the CharSelect scenes, 
/// and ensures proper coordination bewteen the two during local and online matches that switch between the two scenes.
/// This manager exists in its own scene that should only be loaded once.
/// Script should be set to execute before basically all other scripts or wlse Awake order may not work.
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
    public PlayerManager playerManager {get; private set;}

    /// <summary>
    /// Handles connection and disconnection of players via devices.
    /// Joining should only be enabled during local multiplayer games.
    /// </summary>
    public PlayerInputManager playerInputManager {get; private set;}


    /// <summary>
    /// (Server/Host only) Current player connection manager being used.
    /// Different playermanagers are used for different gamemodes!
    /// When a gamemode is loaded the corresponding playermanager will be instantiated.
    /// When 
    /// </summary>
    public IServerPlayerConnectionManager serverPlayerConnectionManager {get; private set;}

    /// <summary>
    /// Battle data to use for the current game, such as the RNG seed, cycle length and color count, etc
    /// Sent by the charselectmanager or postgamemanager before starting a new game
    /// </summary>
    public BattleData battleData {get; private set;}

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

    /// <summary>
    /// Current method of connecting players.
    /// If this is anything other than None when this GameManager's Start is called,
    /// a battle with that mode will automatically be started with that connection mode.
    /// This can be used for easy testing in the editor without having to go throughthe match setup and character select process first.
    /// </summary>
    [SerializeField] private GameConnectionType _currentConnectionType = GameConnectionType.None;
    public GameConnectionType currentConnectionType => _currentConnectionType;


    public enum GameState {
        None,
        CharSelect,
        Countdown,
        Playing,
        Paused,
        PostGame
    }

    /// <summary>
    /// Current state that the game is in.
    /// </summary>
    private GameState _currentGameState = GameState.None;
    public GameState currentGameState => _currentGameState;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Duplicate GameManager spawned! Destroying the new one.");
            Destroy(gameObject);
        }

        networkManager = GetComponent<NetworkManager>();
        playerManager = GetComponent<PlayerManager>();
        playerInputManager = GetComponent<PlayerInputManager>();
        playerInputManager.enabled = false;

        networkManager.OnServerStarted += OnServerStarted;
        networkManager.OnServerStopped += OnServerStopped;
        networkManager.OnClientStarted += OnClientStarted;
        networkManager.OnClientStopped += OnClientStopped;
    }

    public void SetConnectionType(GameConnectionType connectionType) {
        _currentConnectionType = connectionType;
    }

    public void SetGameState(GameState gameState) {
        _currentGameState = gameState;
    }

    public void SetBattleData(BattleData battleData) {
        this.battleData = battleData;
    }

    /// <summary>
    /// Used to start a new game, or join an existing game.
    /// Should be called after the network manager starts.
    /// </summary>
    /// <param name="gameConnectionType">The type of connection used to connect with other players</param>
    /// <param name="host">Start host if true, start client if false (should only be false if connecting to another player in online mode)</param>
    public void StartGameHost(GameConnectionType gameConnectionType) {
        // Network manager needed for GameManager to work properly
        if (!NetworkManager.Singleton) {
            Debug.LogError("Trying to start a game, but there is no NetworkManager present!");
            return;
        }

        // Can't start a game if network manager is already active, meaning a game is already happening
        if (NetworkManager.Singleton.IsListening) {
            Debug.LogError("Trying to start a new game, but network manager is already listening, a game may already be in progress!");
            return;
        }

        _currentConnectionType = gameConnectionType;
        _currentGameState = GameState.CharSelect;
        Debug.Log("Starting a game as a host with connection type "+_currentConnectionType+"...");

        // Start the appropriate player connection manager based on the connection type of the game
        if (gameConnectionType == GameConnectionType.OnlineMultiplayer) {
            serverPlayerConnectionManager = new OnlinePlayerConnectionManager();
            // Start a host if in online mode; so this client can have its own player
            NetworkManager.Singleton.StartHost();
        }
        else if (gameConnectionType == GameConnectionType.LocalMultiplayer) {
            serverPlayerConnectionManager = GetComponent<LocalPlayerConnectionManager>();
            // Start server, because this client shouldnt have just one player, but rather a player for each device
            NetworkManager.Singleton.StartServer();
            playerInputManager.enabled = true;
        }
        else {
            serverPlayerConnectionManager = new SingleplayerConnectionManager();
            // Start a host; this client's player is the one singleplayer player
            NetworkManager.Singleton.StartHost();
        }

        if (!NetworkManager.Singleton.IsListening) {
            Debug.LogError("Failed to start networkmanager as a host; stopping game");
            return;
        }
    }

    /// <summary>
    /// Join another online game as a client.
    /// Online multiplayer only.
    /// </summary>
    public void JoinGameClient() {
        _currentConnectionType = GameConnectionType.OnlineMultiplayer;
        _currentGameState = GameState.CharSelect;

        if (!NetworkManager.Singleton) {
            Debug.LogError("Trying to join a game, but there is no NetworkManager present!");
            return;
        }

        if (NetworkManager.Singleton.IsListening) {
            Debug.LogError("Trying to join a game, but network manager is already listening, a game may already be in progress!");
            return;
        }

        Debug.Log("Joining an online game...");

        NetworkManager.Singleton.StartClient();
        
    }


    public void LeaveGame() {
        if (networkManager.IsServer) {
            StopGameHost();
        } else {
            ClientDisconnectFromGame();
        }
    }

    /// <summary>
    /// Stop the current game. Used when a game is no longer needed (going from battle/charselect to main menu or other non-game scene).
    /// Current game must be stopped before another game can be started.
    /// </summary>
    public void StopGameHost() {
        // Can't start a game if network manager is already active, meaning a game is already happening
        if (!networkManager.IsListening) {
            Debug.LogError("Trying to stop a game, but network manager is not listening, there is no game to end");
            return;
        }

        playerInputManager.enabled = false;
        serverPlayerConnectionManager.StopListeningForPlayers();
        if (currentConnectionType == GameConnectionType.OnlineMultiplayer) LobbyManager.Instance.LeaveLobby();
        networkManager.Shutdown();
    }

    public void ClientDisconnectFromGame() {
        if (!NetworkManager.Singleton.IsListening) {
            Debug.LogError("Trying to disconnect from a game, but network manager is not listening, there is no game to end");
            return;
        }

        if (currentConnectionType == GameConnectionType.OnlineMultiplayer) LobbyManager.Instance.LeaveLobby();
        networkManager.Shutdown();
    }

    public void OnServerStarted() {
        Debug.Log("Server started");
        if (currentConnectionType == GameConnectionType.LocalMultiplayer) {
            if (CharSelectManager.Instance) CharSelectManager.Instance.ShowCharSelectMenu();

            if (networkManager.IsServer) {
                serverPlayerConnectionManager.StartListeningForPlayers();
            }
        }        
    }

    public void OnServerStopped(bool wasHost) {
        Debug.Log("Server stopped");
        if (currentConnectionType == GameConnectionType.LocalMultiplayer) {
            _currentGameState = GameState.None;
            if (CharSelectManager.Instance) CharSelectManager.Instance.ShowConnectionMenu();
            else if (BattleManager.Instance) SceneManager.LoadScene("CharSelect");
        }        
    }

    public void OnClientStarted() {
        Debug.Log("Client started");
        if (CharSelectManager.Instance) CharSelectManager.Instance.ShowCharSelectMenu();

        if (networkManager.IsServer) {
            serverPlayerConnectionManager.StartListeningForPlayers();
        }
    }

    public void OnClientStopped(bool wasHost) {
        Debug.Log("Client stopped");
        _currentGameState = GameState.None;
        if (currentConnectionType == GameConnectionType.OnlineMultiplayer) {
            if (CharSelectManager.Instance) CharSelectManager.Instance.ShowConnectionMenu();
            else if (BattleManager.Instance) SceneManager.LoadScene("CharSelect");
        }
    }

    public void OnClientDisconnected() {

    }

    /// <summary>
    /// Returns true if game type is not the default value, meaning a game is already started
    /// </summary>
    public bool IsGameActive() {
        return currentConnectionType != GameConnectionType.None;
    }
}