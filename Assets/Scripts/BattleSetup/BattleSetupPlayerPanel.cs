using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// A BattleSetupPlayerPanel controls a single player and the panel in the BattleSetup for choosing character and other settings.
/// </summary>
public class BattleSetupPlayerPanel : NetworkBehaviour
{
    /// <summary>
    /// Determines whether or not this player is ready to start the game.
    /// </summary>
    public NetworkVariable<bool> ready = new NetworkVariable<bool>();

    /// <summary>
    /// checkbox for when the player is ready
    /// (PROBABLY TEMPORARY, will be replaced with selecting a character, RNG and drop speed.)
    /// </summary>
    [SerializeField] private Toggle toggle;

    private void OnEnable() {
        toggle.onValueChanged.AddListener(OnReadyToggleChanged);
    }

    private void OnDisable() {
        toggle.onValueChanged.RemoveListener(OnReadyToggleChanged);
    }

    public void InitializeBattleSetup(CharacterSelectMenu characterSelectMenu) {
        if (BattleNetworkManager.instance.IsHost) {
            Debug.Log("Spawning "+this+" on network");
            // "Spawn" on the network so that values such as Ready are synced to clients
            GetComponent<NetworkObject>().Spawn();
        } else {
            Debug.Log("We are not the host, don't spawn the player panels");
        }
    }

    public override void OnNetworkSpawn() {
        Debug.Log(this+" spawned");
        ready.OnValueChanged += OnReadyChanged;
    }

    public override void OnNetworkDespawn() {
        ready.OnValueChanged -= OnReadyChanged;
    }

    /// <summary>
    /// For when the ready toggle is changed (PROBABLY TEMPORARY)
    /// </summary>
    public void OnReadyToggleChanged(bool value) {
        ready.Value = value;
    }

    /// <summary>
    /// For when the ready networkvariable is changed
    /// </summary>
    public void OnReadyChanged(bool previous, bool current) {
        toggle.isOn = current;
    }
}
