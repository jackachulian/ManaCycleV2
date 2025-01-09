using UnityEngine;
using UnityEngine.InputSystem;
using Battle;
using System.Collections.Generic;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;

// ties multiple systems in the CSS together
public class CharSelectManager : MonoBehaviour
{
    public static CharSelectManager Instance { get; private set; }

    [SerializeField] private CharSelector[] _charSelectors;
    public CharSelector[] charSelectors => _charSelectors;
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
            DontDestroyOnLoad(gameObject);
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
            } else {
                GameManager.Instance.StartGameHost(GameManager.Instance.currentConnectionType);
            }
        }

        // if not, open the connection menu and wait for player to join an online game
        else {
            ShowConnectionMenu();
        }
    }

    public CharSelector GetCharSelector(int boardIndex) {
        return _charSelectors[boardIndex];
    }

    // TODO: maybe use animations to make a better transition between these menus
    public void ShowConnectionMenu() {
        // Use CanvasGroups instead of disabling the object so that the objects' Awake can call properly to initialize them before a battle starts
        charSelectMenuUi.GetComponent<CanvasGroup>().alpha = 0;
        charSelectMenuUi.GetComponent<CanvasGroup>().blocksRaycasts = false;

        connectionMenuUi.GetComponent<CanvasGroup>().alpha = 1;
        connectionMenuUi.GetComponent<CanvasGroup>().blocksRaycasts = true;
        connectionMenuUi.connectionMenuEventSystem.enabled = true;
    }

    public void ShowCharSelectMenu() {
        connectionMenuUi.GetComponent<CanvasGroup>().alpha = 0;
        connectionMenuUi.GetComponent<CanvasGroup>().blocksRaycasts = false;
        connectionMenuUi.connectionMenuEventSystem.enabled = false;


        charSelectMenuUi.GetComponent<CanvasGroup>().alpha = 1;
        charSelectMenuUi.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
