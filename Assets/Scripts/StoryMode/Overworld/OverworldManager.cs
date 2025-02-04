using StoryMode.Overworld;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldManager : MonoBehaviour {
    public static OverworldManager Instance;


    [SerializeField] private StoryMenu _storyMenu;
    public StoryMenu storyMenu => _storyMenu;


    public InputActionReference storyMenuToggleAction;


    void Awake() {
        if (Instance) {
            Debug.LogWarning("Duplicate OverworldManager");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        storyMenuToggleAction.action.performed += OnStoryMenuTogglePressed;
        storyMenu.onHide += OnStoryMenuClosed;
    }

    public void OnStoryMenuTogglePressed(InputAction.CallbackContext ctx) {
        if (!storyMenu.showing 
            && OverworldPlayer.Instance.ActiveState != OverworldPlayer.PlayerState.Menu 
            && OverworldPlayer.Instance.ActiveState != OverworldPlayer.PlayerState.Convo
        )
        {
            OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.Menu);
            storyMenu.ControlMenu();
        }

        // if story menu is active, close it and any sub menus it has opened
        else if (storyMenu.showing) {
            storyMenu.StopControllingDeferred();
        }
    }

    public void OnStoryMenuClosed() {
        OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.Movement);
    }
}