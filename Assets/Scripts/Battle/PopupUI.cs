using TMPro;
using UnityEngine;

public class PopupUI : MonoBehaviour {
    [SerializeField] private TMP_Text countText;
    [SerializeField] private SpriteRenderer durationBar;

    void Start() {
        gameObject.SetActive(false);
    }

    public void Show(int count) {
        gameObject.SetActive(true);
        countText.text = count+"";
    }

    public void Hide() {
        // todo: probably want to adjust the opacity of the entire popup over time, instead of disabling the object after a delay
        // await Awaitable.WaitForSecondsAsync(1f);
        gameObject.SetActive(false);
    }

    public void DisplayTimeLeft(float timePercentageLeft) {
        durationBar.size = new Vector2(40 * timePercentageLeft, durationBar.size.y);
    }
}