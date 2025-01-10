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
    /// <summary>
    /// The cursor that belongs to this charselector. There is one attached to each charselector.
    /// Only enable this if a local player is controlling this char selector, otherwise it isn't needed.
    /// </summary>
    [SerializeField] private Cursor _cursor;
    public Cursor cursor => _cursor;

    [Header("Main UI")]
    [SerializeField] private Image battlerPortrait;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text playerIdText; // only for debugging, shouldnt be in full release
    [SerializeField] [FormerlySerializedAs("nameText")] private TMP_Text battlerNameText;

    [Header("Status Strings")]
    [SerializeField] private string unconnectedLocalText = "<Press Button To Connect>";
    [SerializeField] private string unconnectedOnlineText = "Waiting for players...";
    [SerializeField] private string disconnectedText = "Disconnected";
    [SerializeField] private string localSelectText = "Select a Character";
    [SerializeField] private string onlineSelectText = "Selecting...";

    [Header("Colors")]
    [SerializeField] private Color connectedTextColor;
    [SerializeField] private Color unconnectedTextColor;

    [Header("Menus")]
    [SerializeField] private GameObject optionsWindow;
    [SerializeField] private GameObject readyWindow;
    [SerializeField] [FormerlySerializedAs("firstOption")] private GameObject optionsFirstSelected;


    private RectTransform portriatRectTransform;
    private Vector2 defaultPos;
    private CharSelector charSelector;


    void Awake()
    {
        charSelector = GetComponent<CharSelector>();
        portriatRectTransform = battlerPortrait.GetComponent<RectTransform>();
        defaultPos = portriatRectTransform.anchoredPosition;
    }
    
    void Start() {
        // when starting, call OnAssignedPlayer while there is no player assigned so that graphics show no player and no selected battler
        OnAssignedPlayer();
    }

    /// <summary>
    /// Call this whenever a new player is assigned or a player is removed.
    /// </summary>
    public void OnAssignedPlayer() {
        // If there is a player and the client owns the player, enable the cursor, otherwise disable it.
        if (charSelector.player && charSelector.player.IsOwner)  {
            cursor.gameObject.SetActive(true);
            cursor.SetPlayer(charSelector.player);
        } else {
            cursor.gameObject.SetActive(false);
            cursor.SetPlayer(null);
        }

        Debug.Log("Updating UI after player assign!");

        UpdatePlayerName();
        UpdateSelectedBattler();
        UpdateReadinessStatus();
    }

    /// <summary>
    /// Call this whenever the selected battler index of the charselector changes.
    /// </summary>
    public void UpdateSelectedBattler() {
        Battler battler = CharSelectManager.Instance.GetBattlerByIndex(charSelector.selectedBattlerIndex.Value);
        if (battler) {
            battlerPortrait.sprite = battler.sprite;
            portriatRectTransform.anchoredPosition = defaultPos + battler.portraitOffset;
            battlerNameText.color = connectedTextColor;
            battlerNameText.text = battler.displayName;

            // Grey out if not fully ready
            battlerPortrait.color = new Color(1f, 1f, 1f, charSelector.optionsChosen.Value ? 1f : 0.5f);
        } else {
            battlerPortrait.sprite = null;
            battlerPortrait.color = new Color(1f, 1f, 1f, 0f);

            if (charSelector.player) {
                // Show "Select a Character" if a local palyer controls this selector
                // show "Selecting..." if controlled by a remote player
                battlerNameText.color = connectedTextColor;
                battlerNameText.text = charSelector.IsOwner ? localSelectText : onlineSelectText;
            } else {
                battlerNameText.color = unconnectedTextColor;
                battlerNameText.text = GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.LocalMultiplayer ? unconnectedLocalText : unconnectedOnlineText;
            }
        }
    }

    /// <summary>
    /// Call this whenever the charselector's characterChosen or optionsChosen bool changes.
    /// </summary>
    public void UpdateReadinessStatus() {
        // Show the options menu if character is chosen but options are not
        if (charSelector.characterChosen.Value && !charSelector.optionsChosen.Value) {
            optionsWindow.SetActive(true);
            battlerNameText.enabled = false;

            // If player is locally controlled, select the first option with the player's event system 
            // (may want to do after 1 frame delay because the event system is dumb)
            if (charSelector.IsOwner && charSelector.player) {
                MultiplayerEventSystem multiplayerEventSystem = charSelector.player.GetComponent<MultiplayerEventSystem>();
                multiplayerEventSystem.SetSelectedGameObject(null);
                multiplayerEventSystem.SetSelectedGameObject(optionsFirstSelected);
                Debug.Log("Selecting first option");
            }
        } else {
            optionsWindow.SetActive(false);
            battlerNameText.enabled = true;
        }

        if (charSelector.optionsChosen.Value) {
            readyWindow.SetActive(true);
        } else {
            readyWindow.SetActive(false);
        }

        // Only check for cursor unlock if this player is on the local client, otherwise they won't use a cursor here if they are a remote player
        if (charSelector.IsOwner) {
            // Cursor is only unlocked (moveable) if character not already selected
            if (!charSelector.characterChosen.Value) {
                cursor.SetLocked(false);
            } else {
                cursor.SetLocked(true);
            }
        }
    }

    /// <summary>
    /// Should be called whenever player data (username, etc) is changed, or a player is assigned or unassigned.
    /// </summary>
    public void UpdatePlayerName() {
        if (!charSelector.player) {
            usernameText.text = "";
            playerIdText.text = "";
            return;
        }

        playerIdText.text = "ID: "+charSelector.player.playerId.Value;

        // local multiplayer - show the playernumber and the device name, may change to just player number
        if (GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.LocalMultiplayer) {
            int playerNumber = charSelector.player.playerInput.playerIndex + 1;
            var deviceName = charSelector.player.playerInput.devices[0].shortDisplayName;
            if (deviceName == null || deviceName == "") deviceName = charSelector.player.playerInput.devices[0].displayName;
            if (deviceName == null || deviceName == "") deviceName = charSelector.player.playerInput.devices[0].name;
            if (deviceName == "Mouse") deviceName = "Keyboard";
            usernameText.text = "P"+playerNumber+" - "+deviceName;
        }

        else if (GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.OnlineMultiplayer) {
            // TODO: implement this!
            usernameText.text = "username123";
        }
    }
}
