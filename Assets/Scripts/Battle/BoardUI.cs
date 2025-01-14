using Battle;
using UnityEngine;

/// <summary>
/// Handles the battler portrait sprite, fall anim, and any other board-specific visual elements on the board
/// </summary>
public class BoardUI : MonoBehaviour {
    [SerializeField] private SpriteRenderer portraitSpriteRenderer;

    public void ShowBattler(Battler battler) {
        if (battler) {
            portraitSpriteRenderer.sprite = battler.sprite;
        } else {
            portraitSpriteRenderer.sprite = null;
        }
    }
}