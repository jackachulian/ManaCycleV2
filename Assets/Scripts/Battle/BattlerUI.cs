using Battle;
using UnityEngine;

/// <summary>
/// Handles the battler portrait sprite and any other battler-specific visual elements on the board
/// </summary>
public class BattlerUI : MonoBehaviour {
    [SerializeField] private SpriteRenderer portraitSpriteRenderer;

    public void ShowBattler(Battler battler) {
        if (battler) {
            portraitSpriteRenderer.sprite = battler.sprite;
        } else {
            portraitSpriteRenderer.sprite = null;
        }
    }
}