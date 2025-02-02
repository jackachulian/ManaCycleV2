using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles earning score in singleplayer mode for Score-type levels
/// </summary>
public class ScoreManager : MonoBehaviour {
    /// <summary>
    /// Entire score ui object. Should be hidden when incoming damage is being used. Score will still be tracked but not shown.
    /// </summary>
    [SerializeField] private GameObject scoreUiObject;
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private int scoreMinDigits = 6;

    /// <summary>
    /// Set on init. Will only be true if there is only 1 player as the game starts.
    /// </summary>
    private bool showingScoreUI;

    /// <summary>
    /// Current score. Used for score requirements in levels.
    /// </summary>
    public int score {get; private set;} = 0;

    private Board board;

    /// <summary>
    /// Called when the battle initializes, after the ManaCycle and the Board for this healthmanager is initialized.
    /// </summary>
    /// <param name="board"></param>
    public void InitializeBattle(Board board) {
        this.board = board;
        showingScoreUI = GameManager.Instance.playerManager.players.Count <= 1;
        scoreUiObject.SetActive(showingScoreUI);
        if (showingScoreUI) UpdateScoreUI();
    }

    /// <summary>
    /// Adds to the score total and updates the UI.
    /// </summary>
    /// <param name="score">amount of score gained</param>
    public void AddScore(int score) {
        this.score += score;
        if (showingScoreUI) UpdateScoreUI();
    }

    public void UpdateScoreUI() {
        scoreLabel.text = (score+"").PadLeft(scoreMinDigits, '0');
    }
}