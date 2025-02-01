using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PostGameMenuUI : MonoBehaviour {
    [SerializeField] private EventSystem uiEventSystem;
    [SerializeField] private GameObject firstSelectedObject;
    [SerializeField] private Animator animator;

    [Header("Visual Assets")]
    [SerializeField] private Image battlerPortrait;
    [SerializeField] private Image[] battlerColoredImages;
    [SerializeField] private TMP_Text nameText;

    private bool menuShown = false;

    void Awake() {
        if (!menuShown) gameObject.SetActive(false);
    }


    public void SetAssets(Battler winnerBattler)
    {
        if (winnerBattler) {
            battlerPortrait.sprite = winnerBattler.sprite;
            foreach (var i in battlerColoredImages) i.color = winnerBattler.mainColor;
            nameText.text = winnerBattler.displayName + " Wins";
        } else {
            battlerPortrait.sprite = null;
            battlerPortrait.color = Color.clear;
            // foreach (var i in battlerColoredImages) i.color = winnerBattler.mainColor;
            nameText.text = "Defeat...";
        }
    }

    /// <summary>
    /// Show the postgame menu and a winner if there was one.
    /// </summary>
    /// <param name="winner">the winner of the game. Null if no player won the game (singleplayer loss).</param>
    public void ShowPostGameMenuUI(Board shownBattlerBoard) {
        SetAssets(shownBattlerBoard ? shownBattlerBoard.player.battler : null);
        menuShown = true;
        gameObject.SetActive(true);
        animator.ResetTrigger("Open");
        animator.SetTrigger("Open");

    }

    public void OnRematchPressed() {
        BattleManager.Instance.gameStartNetworkBehaviour.RematchRpc();
    }

    public void OnCharacterSelectPressed() {
        BattleManager.Instance.gameStartNetworkBehaviour.GoToCharacterSelectRpc();
    }

    public void OnMainMenuPressed() {
        GameManager.Instance.LeaveGame();
        TransitionManager.Instance.TransitionToScene("MainMenu", "ReverseWipe");
    }

    public void OpenAnimationComplete()
    {
        uiEventSystem.enabled = true;
        uiEventSystem.SetSelectedGameObject(null);
        uiEventSystem.SetSelectedGameObject(firstSelectedObject);
    }
}