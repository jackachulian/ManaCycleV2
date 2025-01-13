using UnityEngine;
using UnityEngine.EventSystems;
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
        EventSystem.current.SetSelectedGameObject(firstSelectedObject);
    }

    public void OnRematchPressed() {
        battleManager.gameStartNetworkBehaviour.RematchRpc();
    }

    public void OnCharacterSelectPressed() {
        battleManager.gameStartNetworkBehaviour.GoToCharacterSelectRpc();
    }

    public void OnMainMenuPressed() {
        GameManager.Instance.LeaveGame();
        SceneManager.LoadScene("MainMenu");
    }
}