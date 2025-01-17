using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// TODO: Eventually this class will show and hide the 3rd and 4th board based on the set room size.
/// Haven't implemented that yet though.
/// </summary>
public class CharSelectorLayout : MonoBehaviour
{
    [SerializeField] private CharSelector[] _selectors;
    public CharSelector[] selectors => _selectors;

    void OnEnable()
    {
        UpdateLayout();
        GameManager.Instance.playerManager.onPlayerSpawned += OnPlayerSpawnedOrDespawned;
        GameManager.Instance.playerManager.onPlayerDespawned += OnPlayerSpawnedOrDespawned;
    }

    void OnDisable()
    {
        GameManager.Instance.playerManager.onPlayerSpawned -= OnPlayerSpawnedOrDespawned;
        GameManager.Instance.playerManager.onPlayerDespawned -= OnPlayerSpawnedOrDespawned;
    }

    public void OnPlayerSpawnedOrDespawned(Player player)
    {
        UpdateLayout();
    }
    
    public void UpdateLayout()
    {
        // First two selectors will always be visible; don't touch those

        // 3rd will be shown if there are 2 or more players, to have one open to connect
        if (GameManager.Instance.playerManager.players.Count >= 2)
        {
            selectors[2].ShowSelector();
        } 
        else
        {
            selectors[2].HideSelector();
        }

        // 4th will be shown if there are 3 or more players, to have one open to connect
        if (GameManager.Instance.playerManager.players.Count >= 3)
        {
            selectors[3].ShowSelector();
        }
        else
        {
            selectors[3].HideSelector();
        }
    }

    public CharSelector GetCharSelectorByIndex(int boardIndex) {
        return _selectors[boardIndex];
    }
}
