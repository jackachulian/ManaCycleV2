using StoryMode.Overworld;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldManager : MonoBehaviour {
    public static OverworldManager Instance;


    [SerializeField] private StoryMenu _storyMenu;
    public StoryMenu storyMenu => _storyMenu;


    public InputActionReference storyMenuOpenAction;


    void Awake() {
        if (Instance) {
            Debug.LogWarning("Duplicate OverworldManager");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        storyMenuOpenAction.action.performed += OpenStoryMenuOpenPressed;
    }

    public void OpenStoryMenuOpenPressed(InputAction.CallbackContext ctx) {
        if (!storyMenu.menuActive && OverworldPlayer.Instance.ActiveState != OverworldPlayer.PlayerState.Menu) {
            storyMenu.OpenMenu();
        }
    }
}