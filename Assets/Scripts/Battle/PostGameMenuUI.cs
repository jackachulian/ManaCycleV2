using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class PostGameMenuUI : MonoBehaviour {
    public BattleManager battleManager;
    [SerializeField] private GameObject firstSelectedObject;

    void Awake() {
        gameObject.SetActive(false);
    }

    public void ShowPostGameMenuUI() {
        gameObject.SetActive(true);
    }

    public void OnRematchPressed() {

    }

    public void OnCharacterSelectPressed() {
        battleManager.gameStartNetworkBehaviour.RematchRpc();
    }

    public void OnMainMenuPressed() {
        GameManager.Instance.LeaveGame();
        SceneManager.LoadScene("MainMenu");
    }

    public void OnLeaveLobbyPressed() {

    }
}