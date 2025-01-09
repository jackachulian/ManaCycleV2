using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Battle;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;

/// <summary>
/// Handles the visuals for a charselector.
/// </summary>
public class CharSelectorUI : MonoBehaviour
{
    [SerializeField] private Image battlerPortrait;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] [FormerlySerializedAs("defaultText")] private string unconnectedText;
    [SerializeField] private string selectText;
    [SerializeField] private GameObject optionsWindow;
    [SerializeField] private GameObject readyWindow;
    [SerializeField] private GameObject firstOption;
    private RectTransform portriatRectTransform;
    private Vector2 defaultPos;


    /// <summary>
    /// true if choice locked in, false if still moving the cursor to choose a character
    /// </summary>
    public bool characterChoiceConfirmed {get; set;} = false;

    void Awake()
    {
        portriatRectTransform = battlerPortrait.GetComponent<RectTransform>();
        defaultPos = portriatRectTransform.anchoredPosition;
    }

    void Start()
    {
        SetBattler(null);
        ShowUnconnectedText();
    }

    public void ShowSelectText() {
        SetBattler(null);
        nameText.text = selectText;
    }

    public void ShowUnconnectedText() {
        SetBattler(null);
        nameText.text = unconnectedText;
    }

    /// <summary>
    /// Show a battler on this charselector, or show nothing if battler is null.
    /// </summary>
    /// <param name="battler">the battler to show the portrait and name of, or null to show no battler</param>
    public void SetBattler(Battler battler)
    {
        if (battler) {
            battlerPortrait.sprite = battler.sprite;
            portriatRectTransform.anchoredPosition = defaultPos + battler.portraitOffset;
            nameText.text = battler.displayName;
            SetLockedVisual();
        } else {
            battlerPortrait.sprite = null;
            battlerPortrait.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    /// <summary>
    /// Display the portrait differently based on whether or not the player has locked in their player choice.
    /// </summary>
    public void SetLockedVisual()
    {
        battlerPortrait.color = new Color(1f, 1f, 1f, characterChoiceConfirmed ? 1f : 0.5f);
    }

    /// <summary>
    /// Open the options menu and have the player select the first option.
    /// </summary>
    /// <param name="multiplayerEventSystem">the multiplayer event system of the player using the options menu</param>
    public void OpenOptions(MultiplayerEventSystem multiplayerEventSystem)
    {
        optionsWindow.SetActive(true);
        multiplayerEventSystem.SetSelectedGameObject(firstOption);
        multiplayerEventSystem.enabled = true;
    }

    public void CloseOptions(MultiplayerEventSystem multiplayerEventSystem)
    {
        optionsWindow.SetActive(false);
        multiplayerEventSystem.SetSelectedGameObject(null);
        multiplayerEventSystem.enabled = false;
    }

    public void ShowReadyVisual() {
        readyWindow.SetActive(true);
    }

    public void HideReadyVisual() {
        readyWindow.SetActive(false);
    }
}
