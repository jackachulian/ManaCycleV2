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

    [SerializeField] [FormerlySerializedAs("portraits")] private CharSelector[] charSelectors;
    [SerializeField] private GameObject allReadyWindow;
    private bool allReady = false;

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
    }

    private void Start() {
        if (!GameManager.Instance) {
            Debug.LogError("A GameManager is required for CharSelectManager to work!");
            return;
        }

        // If the current connection type is CharSelect when this starts,
        // that means the editor is being tested straight into a game without the connection menu.
        // so automatically start a game if so
        if (GameManager.Instance.currentGameState == GameManager.GameState.CharSelect) {
            if (GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.None) {
                Debug.Log("Game state set but no connection type set, auto selecting local multiplayer");
                GameManager.Instance.StartGameHost(GameManager.GameConnectionType.LocalMultiplayer);
            } else {
                GameManager.Instance.StartGameHost(GameManager.Instance.currentConnectionType);
            }
        }
    }

    public void OnReadyStateChanged()
    {
        foreach (CharSelector selector in charSelectors)
        {
            if (!selector.IsReady()) {
                allReadyWindow.SetActive(false);
                allReady = false;
            }
        }

        allReady = true;
        allReadyWindow.SetActive(true);
    }

    public CharSelector GetCharSelector(int boardIndex) {
        return charSelectors[boardIndex];
    }
}
