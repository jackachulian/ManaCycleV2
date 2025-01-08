using UnityEngine;

public class PostGameMenuUI : MonoBehaviour {
    public BattleManager battleManager;

    public void ShowPostGameMenuUI() {
        // TODO: implement the menu.
        // For now, just take both players back to the character select screen if in a multiplayer game.
        // This might end up being what happens in online mode anyways.
        Debug.Log("Post game UI not implemented yet");
    }

    public void OnRematchPressed() {

    }

    public void OnCharacterSelectPressed() {
        battleManager.GoToCharacterSelect();
    }

    public void OnLeaveLobbyPressed() {

    }
}