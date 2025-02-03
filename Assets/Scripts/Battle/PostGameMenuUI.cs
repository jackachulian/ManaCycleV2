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

    [Header("Buttons")]
    [SerializeField] private Button charSelectButton;
    [SerializeField] private Button levelSelectButton;

    private bool menuShown = false;

    void Awake() {
        if (!menuShown) gameObject.SetActive(false);
    }


    public void SetAssets(Battler battler, bool won)
    {
        if (battler) {
            battlerPortrait.sprite = battler.sprite;
            foreach (var i in battlerColoredImages) i.color = battler.mainColor;
        } else {
            battlerPortrait.sprite = null;
            battlerPortrait.color = Color.clear;
        }

        if (won) {
            nameText.text = battler.displayName + " Wins";
        } else {
            nameText.text = "Defeat...";
        }
    }

    /// <summary>
    /// Show the postgame menu and a winner if there was one.
    /// </summary>
    /// <param name="winner">the winner of the game. Null if no player won the game (singleplayer loss).</param>
    public void ShowPostGameMenuUI(Board shownBattlerBoard) {
        SetAssets(shownBattlerBoard ? shownBattlerBoard.player.battler : null, shownBattlerBoard.won);
        menuShown = true;

        bool inLevel = GameManager.Instance.level != null;
        charSelectButton.gameObject.SetActive(!inLevel);
        levelSelectButton.gameObject.SetActive(inLevel);

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

    public void OnLevelSelectPressed() 
    {
        GameManager.Instance.LeaveGame();
        TransitionManager.Instance.TransitionToScene("StoryOverworld", "ReverseWipe");
    }

    public void OnMainMenuPressed() {
        GameManager.Instance.LeaveGame();
        TransitionManager.Instance.TransitionToScene("MainMenu", "ReverseWipe");
    }

    public void OpenAnimationComplete()
    {
        GameManager.Instance.playerManager.DisablePlayerInputs();

        uiEventSystem.enabled = true;
        uiEventSystem.gameObject.SetActive(false);
        uiEventSystem.gameObject.SetActive(true);
        uiEventSystem.SetSelectedGameObject(null);
        uiEventSystem.SetSelectedGameObject(firstSelectedObject);
    }
}