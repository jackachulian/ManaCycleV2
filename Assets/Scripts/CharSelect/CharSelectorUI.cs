using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Battle;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the visuals for a charselector.
/// </summary>
public class CharSelectorUI : MonoBehaviour
{
    [SerializeField] private Image battlerPortrait;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] [FormerlySerializedAs("nameText")] private TMP_Text battlerNameText;
    [SerializeField] private string unconnectedText;
    [SerializeField] private string disconnectedText = "Disconnected";
    [SerializeField] private string selectText;
    [SerializeField] private Color connectedTextColor, unconnectedTextColor;
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
        UpdatePlayerData(null);
    }

    public void ShowSelectText() {
        SetBattler(null);
        battlerNameText.color = connectedTextColor;
        battlerNameText.text = selectText;
    }

    public void ShowUnconnectedText() {
        SetBattler(null);
        battlerNameText.color = unconnectedTextColor;
        battlerNameText.text = unconnectedText;
    }

    /// <summary>
    /// Show the disconnected text, then show the unconnected text ("Press button to join") after a delay
    /// </summary>
    public async void ShowDisconnectedText() {
        SetBattler(null);
        battlerNameText.color = unconnectedTextColor;
        battlerNameText.text = disconnectedText;
        await Awaitable.WaitForSecondsAsync(3.0f);
        ShowUnconnectedText();
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
            battlerNameText.color = connectedTextColor;
            battlerNameText.text = battler.displayName;
            SetLockedVisual();
        } else {
            battlerPortrait.sprite = null;
            battlerPortrait.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    /// <summary>
    /// Should be called whenever player data (username, etc) is changed.
    /// </summary>
    public void UpdatePlayerData(Player player) {
        if (!player) {
            usernameText.text = "";
            return;
        }

        // local multiplayer - show the playernumber and the device name, may change to just player number
        if (GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.LocalMultiplayer) {
            int playerNumber = player.playerInput.playerIndex + 1;
            var deviceName = player.playerInput.devices[0].shortDisplayName;
            if (deviceName == null || deviceName == "") deviceName = player.playerInput.devices[0].displayName;
            if (deviceName == null || deviceName == "") deviceName = player.playerInput.devices[0].name;
            if (deviceName == "Mouse") deviceName = "Keyboard";
            usernameText.text = "P"+playerNumber+" - "+deviceName;
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
