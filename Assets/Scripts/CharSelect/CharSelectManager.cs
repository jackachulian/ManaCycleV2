using UnityEngine;
using Battle;
using UnityEngine.SceneManagement;

// ties multiple systems in the CSS together
public class CharSelectManager : MonoBehaviour
{
    public static CharSelectManager Instance { get; private set; }

    [SerializeField] private CharSelectorLayout _charSelectorLayout;
    public CharSelectorLayout charSelectorLayout => _charSelectorLayout;
    [SerializeField] private CharButtonList charButtonList;
    [SerializeField] private GameObject allReadyWindow;


    [SerializeField] private ConnectionMenuUI _connectionMenuUi;
    public ConnectionMenuUI connectionMenuUi => _connectionMenuUi;

    [SerializeField] private Transform _charSelectMenuUi;
    public Transform charSelectMenuUi => _charSelectMenuUi;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Duplicate CharSelectManager spawned! Destroying the new one.");
            Destroy(gameObject);
        }

        // shenanigan prevention 
        // either connectionmenu or charselectmenu will be shown on Start(),, 
        // just make sure both objects are active so thier canvasgroups work
        charSelectMenuUi.gameObject.SetActive(true);
        connectionMenuUi.gameObject.SetActive(true);
    }

    private void Start() {
        if (!GameManager.Instance) {
            Debug.LogError("A GameManager is required for CharSelectManager to work!");
            return;
        }

        // If the current connection type is CharSelect when this starts,
        // that means the scene is being loaded straight into a game without the connection menu.
        // so automatically start a game if so
        if (GameManager.Instance.currentGameState == GameManager.GameState.CharSelect) {
            ShowCharSelectMenu();

            if (GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.None) {
                Debug.Log("Game state set but no connection type set, auto selecting local multiplayer");
                GameManager.Instance.StartGameHost(GameManager.GameConnectionType.LocalMultiplayer);
            }
        }

        // if type is not already charselect
        else {
            // if connect type is online, open the connection menu and wait for player to join an online game
            if (GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.OnlineMultiplayer) {
                ShowConnectionMenu();
            }
            // if connection type is none, just start a local multiplayer in char select
            else if (GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.None) {
                ShowConnectionMenu();
            }
            // otherwise, in singleplayer and local multiplayer, start a game and go to the char select screen
            else {
                GameManager.Instance.StartGameHost(GameManager.Instance.currentConnectionType);
                ShowCharSelectMenu();
            }
        }

        // will attach players to selectors if entering char select scene from the battle scene
        if (GameManager.Instance.IsGameActive()) {
            GameManager.Instance.playerManager.AttachPlayersToSelectors();
        }
    }

    public void LeaveGamePressed() {
        var connectionType = GameManager.Instance.currentConnectionType;
        GameManager.Instance.LeaveGame();

        // Only take them back to connection menu if mode was online multiplayer
        if (connectionType == GameManager.GameConnectionType.OnlineMultiplayer) {
            // GameManager's OnClientStopped will open the connection menu when the client closes
        }

        // Otherwise, take the player to the previous scene
        else {
            BackToPreviousScene();
        }
    }

    public void BackToPreviousScenePressed() {
        BackToPreviousScene();
    }

    public void BackToPreviousScene() {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Returns the battler with the given ID found on the char button list, 
    /// or null if there is no battler with that ID, or ID id empty/null.
    /// </summary>
    public Battler GetBattlerByIndex(int index) {
        return charButtonList.GetBattlerByIndex(index);
    }

    public CharSelector GetCharSelectorByIndex(int boardIndex) {
        return _charSelectorLayout.GetCharSelectorByIndex(boardIndex);
    }

    public void SetAllReadyWindowActive(bool active) {
        allReadyWindow.SetActive(active);
    }

    // TODO: maybe use animations to make a better transition between these menus
    public void ShowConnectionMenu() {
        if (!connectionMenuUi) return;
        
        // Use CanvasGroups instead of disabling the object so that the objects' Awake can call properly to initialize them before a battle starts
        charSelectMenuUi.GetComponent<CanvasGroup>().alpha = 0;
        charSelectMenuUi.GetComponent<CanvasGroup>().blocksRaycasts = false;

        connectionMenuUi.gameObject.SetActive(true);
        connectionMenuUi.GetComponent<CanvasGroup>().alpha = 1;
        connectionMenuUi.GetComponent<CanvasGroup>().blocksRaycasts = true;
        connectionMenuUi.connectionMenuEventSystem.enabled = true;
        

        // Need to do this because i think either PlayerInputs or MultiplayerEventSystesm are messing up the connection menu event system
        // unity fix your engine
        connectionMenuUi.connectionMenuEventSystem.gameObject.SetActive(false);
        connectionMenuUi.connectionMenuEventSystem.gameObject.SetActive(true);
        connectionMenuUi.connectionMenuEventSystem.SetSelectedGameObject(null);
        
        // var uiInputModule = connectionMenuUi.connectionMenuEventSystem.GetComponent<InputSystemUIInputModule>();
        // var inputActions = uiInputModule.actionsAsset;
        // Destroy(uiInputModule);
        // uiInputModule = connectionMenuUi.connectionMenuEventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        // uiInputModule.actionsAsset = inputActions;

        connectionMenuUi.connectionMenuEventSystem.sendNavigationEvents = true;
    }

    public void ShowCharSelectMenu() {
        if (!charSelectMenuUi) return;

        connectionMenuUi.gameObject.SetActive(false);
        connectionMenuUi.GetComponent<CanvasGroup>().alpha = 0;
        connectionMenuUi.GetComponent<CanvasGroup>().blocksRaycasts = false;
        connectionMenuUi.connectionMenuEventSystem.enabled = false;
        connectionMenuUi.connectionMenuEventSystem.sendNavigationEvents = false;


        charSelectMenuUi.GetComponent<CanvasGroup>().alpha = 1;
        charSelectMenuUi.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
