using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PostGameMenuUI : MonoBehaviour {
    public BattleManager battleManager;
    [SerializeField] private GameObject firstSelectedObject;
    [SerializeField] private Animator animator;
    private bool menuShown = false;

    void Awake() {
        if (!menuShown) gameObject.SetActive(false);
    }

    public void ShowPostGameMenuUI() {
        menuShown = true;
        gameObject.SetActive(true);
        animator.ResetTrigger("Open");
        animator.SetTrigger("Open");

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

    public void OpenAnimationComplete()
    {
        EventSystem.current.SetSelectedGameObject(firstSelectedObject);
    }
}