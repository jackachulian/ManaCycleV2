using UnityEngine;

public class LevelDetailsUI : MonoBehaviour {
    [SerializeField] private Animator animator;

    [SerializeField] private TMPro.TMP_Text levelNameLabel;
    [SerializeField] private TMPro.TMP_Text levelNumberLabel;
    [SerializeField] private TMPro.TMP_Text descriptionLabel;
    [SerializeField] private TMPro.TMP_Text timeLabel;

    public void ShowUI() {
        animator.SetBool("show", true);
    }

    public void HideUI() {
        animator.SetBool("show", false);
    }

    public void DisplayLevel(Level level) {
        levelNameLabel.text = level.name;
        levelNumberLabel.text = level.levelNumber;
        descriptionLabel.text = level.description;
        timeLabel.text = FormatTime(level.timeLimit);
    }

    static string FormatTime(float time, bool showDecimal = false)
    {
        int minutes = (int)(time/60);
        int secondsRemainder = (int)(time % 60);
        int dec = (int)(time * 100 % 100);
        if (showDecimal) {
            return minutes + ":" + (secondsRemainder+"").PadLeft(2, '0')+"<size=40%>."+(dec+"").PadLeft(2, '0');
        } else {
            return minutes + ":" + (secondsRemainder+"").PadLeft(2, '0');
        }
    }
}