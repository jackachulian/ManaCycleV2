using Battle;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles the battler portrait sprite, fall anim, and any other board-specific visual elements on the board
/// </summary>
public class BoardUI : MonoBehaviour {
    [Header("General")]
    [SerializeField] private SpriteRenderer portraitSpriteRenderer;
    
    /// <summary>
    /// shows "WIN!" when winning and "LOSE" when defeated
    /// </summary>
    [SerializeField] private TMP_Text winLoseText;

    // May move defeat fall logic to its own component for better organization
    [Header("Defeat Fall")]
    [SerializeField] private Transform fallTransform;
    [SerializeField] float fallAcceleration = 50f;
    /** rotation angular speed acceleration - degrees/s^2 (starts at 0) */
    [SerializeField] float fallAngularAcceleration = 25f;
    /** Initial falling speed */
    [SerializeField] float initialFallSpeed = -60f;

    /** Starting reference position */
    Vector2 fallStartPos;

    float fallDistance, fallSpeed, rotation, angularSpeed;
    bool falling = false;

    void Start() {
        winLoseText.text = "";

    }

    void Update() {
        if (falling)
        {
            fallDistance += fallSpeed*Time.smoothDeltaTime;
            fallSpeed += fallAcceleration*Time.smoothDeltaTime;
            rotation += angularSpeed*Time.smoothDeltaTime;
            angularSpeed += fallAngularAcceleration*Time.smoothDeltaTime;

            fallTransform.localPosition = fallStartPos + Vector2.down*fallDistance;
            fallTransform.eulerAngles = new Vector3(0, 0, rotation);
        }
    }
    
    public void StartDefeatFall() {
        if (!falling)
        {
            fallStartPos = fallTransform.localPosition;
            fallDistance = 0;
            fallSpeed = initialFallSpeed;
            rotation = 0;
            angularSpeed = 0;
            falling = true;
        }
    }

    public void ShowBattler(Battler battler) {
        if (battler) {
            portraitSpriteRenderer.sprite = battler.sprite;
        } else {
            portraitSpriteRenderer.sprite = null;
        }
    }

    public void ShowWinText() {
        winLoseText.gameObject.SetActive(true);
        winLoseText.text = "Win!";
    }

    public void ShowLoseText() {
        winLoseText.gameObject.SetActive(true);
        winLoseText.text = "Lose";
    }
}