using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour {
    public static LobbyManager Instance {get; private set;}
    public Lobby joinedLobby;
    public string joinedRelayCode;
    [SerializeField] private ConnectionMenuUI connectionMenuUi;


    void Awake() {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Duplicate LobbyManager spawned! Destroying the new one.");
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }

    private async void Start() {
        await LogInIfNotLoggedIn();

        string username = await AuthenticationService.Instance.GetPlayerNameAsync();
        connectionMenuUi.ShowStatus("Logged in as guest: "+username);   

        RefreshLobbies();
    }

    public async Task LogInIfNotLoggedIn() {
        connectionMenuUi.ShowStatus("Initializing Unity Services...");
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            connectionMenuUi.ShowStatus("Signing in as a guest...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void CreateLobby() {
        await LogInIfNotLoggedIn();

        try {
            connectionMenuUi.ShowStatus("Creating new lobby...");
            Lobby createdLobby;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.IsLocked = true; // Lock until Relay is allocated
            options.Data = new Dictionary<string, DataObject> {
                {
                    // join code will be set when after the lobby is created and after relay is allocated
                    "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, string.Empty)
                }
            };
            createdLobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", 4, options);
            joinedLobby = createdLobby;
            LobbyHeartbeat(createdLobby);

            connectionMenuUi.ShowStatus("Creating Relay allocation...");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

            connectionMenuUi.ShowStatus("Getting join code...");
            joinedRelayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            connectionMenuUi.ShowStatus("Setting lobby data...");
            // Unlock the lobby now that Relay is allocated, and add the Relay join code
            UpdateLobbyOptions updatedOptions = new UpdateLobbyOptions();
            updatedOptions.IsLocked = false; 
            updatedOptions.Data = new Dictionary<string, DataObject> {
                {
                    "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinedRelayCode)
                }
            };
            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, updatedOptions);

            connectionMenuUi.ShowStatus("Starting host...");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.ToRelayServerData("dtls"));
            GameManager.Instance.StartGameHost(GameManager.GameConnectionType.OnlineMultiplayer);
        } catch (Exception e) {
            Debug.LogError(e);
            connectionMenuUi.ShowStatus("Error creating lobby: "+e);
        }
    }

    /// <summary>
    /// Disable the refresh button, refresh the lobbies, then allow another refresh after a short delay.
    /// </summary>
    public async void RefreshLobbies() {
        connectionMenuUi.refreshButton.interactable = false;
        QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
        connectionMenuUi.ShowLobbies(queryResponse.Results);
        await Awaitable.WaitForSecondsAsync(2.0f);
        connectionMenuUi.refreshButton.interactable = true;
    }

    // Send a heartbeat every 15 seconds to ensure the lobby doesn't time out (do this on the host)
    public async void LobbyHeartbeat(Lobby lobby) {
        
        while (lobby != null) {
            await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
            await Task.Delay(15000);
        }
    }

    public async void JoinLobby(Lobby lobby) {
        await LogInIfNotLoggedIn();

        try {
            connectionMenuUi.ShowStatus("Joining lobby "+lobby.Id+"...");
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);

            connectionMenuUi.ShowStatus("Joining Relay allocation...");
            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: relayJoinCode);

            connectionMenuUi.ShowStatus("Starting client...");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));
            GameManager.Instance.JoinGameClient();
        } catch (LobbyServiceException e) {
            Debug.Log(e);
            connectionMenuUi.ShowStatus("Could not join lobby: "+e);
        } catch (Exception e) {
            Debug.LogError(e);
            connectionMenuUi.ShowStatus("Error joining lobby: "+e);
        }
    }
}