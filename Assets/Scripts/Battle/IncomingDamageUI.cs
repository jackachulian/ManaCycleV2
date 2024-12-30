using System.Collections.Generic;
using UnityEngine;

public class IncomingDamageUI : MonoBehaviour {
    [SerializeField] private IncomingDamageNumber[] incomingDamageNumbers;

    public void UpdateUI(int[] incomingDamage) {
        for (int i = 0; i < 6; i++) {
            incomingDamageNumbers[i].SetDamage(incomingDamage[i]);
        }
    }
}