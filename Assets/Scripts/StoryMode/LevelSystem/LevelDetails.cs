using System.Threading.Tasks;
using StoryMode.Overworld;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LevelDetails : MonoBehaviour {
    public static LevelDetails Instance;

    [SerializeField] private TMPro.TMP_Text levelNameLabel;
    [SerializeField] private TMPro.TMP_Text levelNumberLabel;
    [SerializeField] private TMPro.TMP_Text descriptionLabel;
    [SerializeField] private TMPro.TMP_Text timeLabel;

    [SerializeField] private GameObject uiObject;
    [SerializeField] private GameObject firstSelected;

    [SerializeField] private InputActionReference cancelAction;

    private Level displayedLevel;

    void Awake() {
        if (Instance) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        uiObject.SetActive(false);
    }

    public async void OpenDetailsWindow(Level level) {
        DisplayLevel(level);

        // OverworldPlayer.Instance.playerInput.SwitchCurrentActionMap("UI");
        OverworldPlayer.Instance.interactionManager.enabled = false;
        OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.Menu);
        // TODO: maybe make this an animator
        uiObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        await Awaitable.EndOfFrameAsync();
        EventSystem.current.SetSelectedGameObject(firstSelected);

        cancelAction.action.performed += OnCancelPressed;
    }

    public void OnCancelPressed(InputAction.CallbackContext ctx) {
        CloseDetailsWindow();
    }

    public async void CloseDetailsWindow() {
        cancelAction.action.performed -= OnCancelPressed;

        EventSystem.current.SetSelectedGameObject(null);
        uiObject.SetActive(false);

        // Double input prevention - wait for cancel action to fall through so it does not unintentionally open the story menu
        await Awaitable.NextFrameAsync();

        // OverworldPlayer.Instance.playerInput.SwitchCurrentActionMap("Overworld");
        OverworldPlayer.Instance.interactionManager.enabled = true;
        if (OverworldPlayer.Instance.ActiveState == OverworldPlayer.PlayerState.Menu) {
            OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.Movement);
        }
    }

    public void PlayLevelPressed() {
        cancelAction.action.performed -= OnCancelPressed;
        
        displayedLevel.StartLevelBattle();
    }

    public void DisplayLevel(Level level) {
        if (!level) return;

        displayedLevel = level;
        levelNameLabel.text = level.name;
        levelNumberLabel.text = level.levelNumber;
        timeLabel.text = FormatTime(level.timeLimit);

        string desc = "Requirements:\n";
        for (int i = 0; i < level.objectiveList.objectives.Count; i++) {
            var objective = level.objectiveList.objectives[i];
            desc += "- "+objective.GetDetailsString();
            if (i < level.objectiveList.objectives.Count-1) desc += "\n";
        }

        descriptionLabel.text = desc;
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