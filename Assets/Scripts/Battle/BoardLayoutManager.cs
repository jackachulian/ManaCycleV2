using UnityEngine;

/// <summary>
/// Manages which layou is used based on the amount of players in the game.
/// </summary>
public class BoardLayoutManager : MonoBehaviour {
    [SerializeField] private BoardLayout singleplayerLayout, twoPlayerLayout, fourPlayerLayout;
    [SerializeField] private BoardLayout _currentLayout;
    public BoardLayout currentLayout => _currentLayout;

    public void DecideLayout() {
        if (_currentLayout) _currentLayout.HideLayout();

        int playerCount = GameManager.Instance.playerManager.players.Count;

        if (playerCount <= 1) {
            _currentLayout = singleplayerLayout;
        } else if (playerCount == 2) {
            _currentLayout = twoPlayerLayout;
        } else {
            _currentLayout = fourPlayerLayout;
        }

        _currentLayout.ShowLayout();
    }
}