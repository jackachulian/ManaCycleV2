using System;
using System.Threading.Tasks;
using SaveDataSystem;
using StoryMode.Overworld;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldManager : MonoBehaviour {
    public static OverworldManager Instance;


    public static Battler activeBattler;
    public static event Action<Battler> onActiveBattlerChanged;


    [SerializeField] private StoryMenu _storyMenu;
    public StoryMenu storyMenu => _storyMenu;
    [SerializeField] private LevelPopup levelPopup;


    public InputActionReference storyMenuToggleAction;


    void Awake() {
        if (Instance) {
            Debug.LogWarning("Duplicate OverworldManager");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable() {
        storyMenuToggleAction.action.performed += OnStoryMenuTogglePressed;
        storyMenu.onHide += OnStoryMenuClosed;
    }

    void OnDisable() {
        storyMenuToggleAction.action.performed -= OnStoryMenuTogglePressed;
        storyMenu.onHide -= OnStoryMenuClosed;
    }

    void Start() {
        if (activeBattler) ChangeActiveBattler(activeBattler);
    }

    public async void OnStoryMenuTogglePressed(InputAction.CallbackContext ctx) {
        if (!storyMenu.showing 
            && OverworldPlayer.Instance.ActiveState != OverworldPlayer.PlayerState.Menu 
            && OverworldPlayer.Instance.ActiveState != OverworldPlayer.PlayerState.Convo
        )
        {
            OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.Menu);
            // levelPopup.StopShowingNearbyLevels();
            levelPopup.Dim();
            storyMenu.rememberObjectSelection = false;
            await storyMenu.ControlMenu();
            storyMenu.rememberObjectSelection = true;
        }

        // // if story menu is active, close it and any sub menus it has opened
        // else if (storyMenu.showing) {
        //     storyMenu.StopControllingDeferred();
        // }
    }

    public void OnStoryMenuClosed() {
        storyMenu.HideMenu();
        // levelPopup.ShowNearbyLevels();
        levelPopup.Undim();
        storyMenu.menuPanelSwapper.HideAllMenus();
        OverworldPlayer.Instance.SetState(OverworldPlayer.PlayerState.Movement);
    }

    public static void ChangeActiveBattler(Battler battler) {
        activeBattler = battler;
        SaveData.current.storyModeData.activeBattlerId = battler.battlerId;

        if (battler.overworldModel && OverworldPlayer.Instance) {
            Destroy(OverworldPlayer.Instance.modelObject);
            GameObject playerModel = Instantiate(battler.overworldModel, OverworldPlayer.Instance.transform);
            OverworldPlayer.Instance.SetPlayerModel(playerModel);
        }

        onActiveBattlerChanged?.Invoke(battler);
    }
}