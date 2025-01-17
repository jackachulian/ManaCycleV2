using UnityEngine;

/// <summary>
/// Manages which layou is used based on the amount of players in the game.
/// </summary>
public class BoardLayoutManager : MonoBehaviour {
    [SerializeField] private BoardLayout singleplayerLayout, twoPlayerLayout, fourPlayerLayout;
    public BoardLayout currentLayout {get; private set;}

    public void DecideLayout() {
        // Start by hiding all layouts
        foreach (Transform child in transform) {
            var layout = child.gameObject.GetComponent<BoardLayout>();
            if (layout) layout.HideLayout();
        }

        int playerCount = GameManager.Instance.playerManager.players.Count;

        if (playerCount <= 1) {
            currentLayout = singleplayerLayout;
        } else if (playerCount == 2) {
            currentLayout = twoPlayerLayout;
        } else {
            currentLayout = fourPlayerLayout;
        }

        currentLayout.ShowLayout();
    }
}