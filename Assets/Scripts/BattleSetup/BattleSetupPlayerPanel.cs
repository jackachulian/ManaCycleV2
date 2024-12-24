using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A BattleSetupPlayerPanel controls a single player and the panel in the BattleSetup for choosing character and other settings.
/// </summary>
public class BattleSetupPlayerPanel : MonoBehaviour
{


    public UnityEvent onReadyChanged;

    private bool _ready;
    public bool ready {
        get {
            return _ready;
        }
        set {
            _ready = value;
            onReadyChanged.Invoke();
        }
    }

    public void InitializeBattleSetup(CharacterSelectMenu characterSelectMenu) {
        

    }
}
