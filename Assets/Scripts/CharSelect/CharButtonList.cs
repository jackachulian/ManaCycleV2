using System.Collections.Generic;
using Battle;
using UnityEngine;

public class CharButtonList : MonoBehaviour {
    /// <summary>
    /// Maps battlerIDs to battlers found on CharButtons that are children of this object
    /// </summary>
    private List<Battler> battlers;

    private void Awake() {
        battlers = new List<Battler>();
        for (int i = 0; i < transform.childCount; i++) {
            CharButton charButton = transform.GetChild(i).GetComponent<CharButton>();
            if (charButton) {
                charButton.index = i;
                battlers.Add(charButton.battler);
            }
        }
    }

    public Battler GetBattlerByIndex(int index) {
        if (index > 0) {
            return battlers[index];
        } else {
            return null;
        }
    }
}