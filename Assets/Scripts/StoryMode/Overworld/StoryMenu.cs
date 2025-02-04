using System.Threading.Tasks;
using StoryMode.Overworld;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class StoryMenu : MonoBehaviour {
    [SerializeField] private GameObject uiObject;
    [SerializeField] private GameObject firstSelected;
    public bool menuActive {get; private set;}

    public InputActionReference menuCancelCloseAction;
    public InputActionReference menuToggleCloseAction;

    void OnEnable() {
        uiObject.SetActive(false);
        menuCancelCloseAction.action.performed += OnClosePressed;
        menuToggleCloseAction.action.performed += OnClosePressed;
    }

    void OnDisable() {
        menuCancelCloseAction.action.performed -= OnClosePressed;
        menuToggleCloseAction.action.performed -= OnClosePressed;
    }

    public void OnClosePressed(InputAction.CallbackContext ctx) {
        if (menuActive) CloseMenu();
    }

    public async void OpenMenu() {
        // bypasses double inputs between esc to open menu and esc to close menu which may be on separate actions
        await Awaitable.NextFrameAsync();

        menuActive = true;
        uiObject.SetActive(true);
        OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.Menu);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void CloseMenu() {
        menuActive = false;
        uiObject.SetActive(false);
        OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.Movement);
    }
}