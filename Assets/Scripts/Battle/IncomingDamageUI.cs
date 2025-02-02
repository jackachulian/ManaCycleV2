using System.Collections.Generic;
using UnityEngine;

public class IncomingDamageUI : MonoBehaviour {
    [SerializeField] private GameObject uiObject;
    [SerializeField] private IncomingDamageNumber[] incomingDamageNumbers;

    public void ShowUI() {
        uiObject.SetActive(true);
    }

    public void HideUI() {
        uiObject.SetActive(false);
    }

    public void UpdateUI(int[] incomingDamage) {
        for (int i = 0; i < 6; i++) {
            incomingDamageNumbers[i].SetDamage(incomingDamage[i]);
        }
    }
}