using StoryMode.Overworld;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelDetails : MonoBehaviour {
    public static LevelDetails Instance;

    [SerializeField] private TMPro.TMP_Text levelNameLabel;
    [SerializeField] private TMPro.TMP_Text levelNumberLabel;
    [SerializeField] private TMPro.TMP_Text descriptionLabel;
    [SerializeField] private TMPro.TMP_Text timeLabel;

    [SerializeField] private GameObject uiObject;
    [SerializeField] private GameObject firstSelected;

    private Level displayedLevel;

    void Awake() {
        if (Instance) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        uiObject.SetActive(false);
    }

    public void OpenDetailsWindow(Level level) {
        DisplayLevel(level);

        OverworldPlayer.Instance.playerInput.SwitchCurrentActionMap("UI");
        OverworldPlayer.Instance.interactionManager.enabled = false;
        OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.LevelDetails);
        // TODO: maybe make this an animator
        uiObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void CloseDetailsWindow() {
        uiObject.SetActive(false);
        OverworldPlayer.Instance.playerInput.SwitchCurrentActionMap("Overworld");
        OverworldPlayer.Instance.interactionManager.enabled = true;
        if (OverworldPlayer.Instance.ActiveState == OverworldPlayer.PlayerState.LevelDetails) {
            OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.Movement);
        }
    }

    public void PlayLevelPressed() {
        displayedLevel.StartLevelBattle();
    }

    public void DisplayLevel(Level level) {
        displayedLevel = level;
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