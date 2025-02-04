using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public abstract class ShowableMenu : MonoBehaviour {
    // Call these from within implementations of ShowableMenus, within the ShowMenu, HideMenu, OpenMenu and CloseMenu methods
    public event Action onShow;
    public event Action onHide;
    public event Action onControlEnter;
    public event Action onControlExit;


    public bool showing {get; private set;}
    public bool controlling {get; private set;}

    /// <summary>
    /// If non-null, stop controlling this window if pressed.
    /// </summary>
    [SerializeField] private InputActionReference backAction;

    protected virtual void Awake() {
        onControlExit += AfterStopControlling;
    }

    /// <summary>
    /// Called when back action is pressed while this is controlled.
    /// </summary>
    public void OnBackPressed(InputAction.CallbackContext ctx) {
        StopControllingMenu();
    }
    
    /// <summary>
    /// UI is shown on the screen in this function
    /// </summary>
    public void ShowMenu() {
        if (showing) return;
        showing = true;
        onShow?.Invoke();
    }

    /// <summary>
    /// UI is hidden on the screen in this function.
    /// </summary>
    public void HideMenu() {
        if (!showing) return;

        // dont control while hidden
        if (controlling) StopControllingMenu();

        showing = false;
        onHide?.Invoke();
    }

    /// <summary>
    /// UIelement is selected and control is given to this menu in this function.
    /// </summary>
    public async void ControlMenu() {
        if (controlling) return;

        // make sure menu is visible before it is controlled
        if (!showing) ShowMenu();

        // wait a frame to prevent double inputs (may change if delay becomes noticeable)
        await Awaitable.NextFrameAsync();

        if (controlling) return;

        Debug.Log("Controlling "+this);
        controlling = true;
        if (backAction) backAction.action.performed += OnBackPressed;
        onControlEnter?.Invoke();
    }

    /// <summary>
    /// UI control is taken away from this menu in this function.
    /// </summary>
    public void StopControllingMenu() {
        if (!controlling) return;
        controlling = false;
        if (backAction) backAction.action.performed -= OnBackPressed;
        onControlExit?.Invoke();
    }

    /// <summary>
    /// If currently deferred to another menu, stop controlling those menus recursively, and then stop controlling this one.
    /// </summary>
    public void StopControllingDeferred() {
        if (deferredMenu) {
            deferredMenu.StopControllingDeferred();
        }

        StopControllingMenu();
    }

    private ShowableMenu deferredMenu = null;
    private GameObject deferredObject = null;

    /// <summary>
    /// Show and control this menu, but return control to passed menu when this menu is closed.
    /// </summary>
    public void ControlMenuDeferred(ShowableMenu menu) {
        if (!CheckIfCanDefer()) return;
        deferredMenu = menu;
        if (!showing) ShowMenu();
        ControlMenu();
    }

    /// <summary>
    /// Show and control this menu, but select the passed object when this menu is closed.
    /// </summary>
    /// <param name="gameObject"></param>
    public void ControlMenuDeferred(GameObject gameObject) {
        if (!CheckIfCanDefer()) return;
        deferredObject = gameObject;
        if (!showing) ShowMenu();
        ControlMenu();
    }

    public bool CheckIfCanDefer() {
        if (deferredMenu) {
            Debug.LogError("Trying to defer "+this+" while it is already deferring to another menu: "+deferredMenu);
            return false;
        }
        if (deferredObject) {
            Debug.LogError("Trying to defer "+this+" while it is already deferring to another object: "+deferredObject);
            return false;
        }

        return true;
    }

    public void AfterStopControlling() {
        if (deferredMenu) {
            deferredMenu.ControlMenu();
        }
        else if (deferredObject) {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(deferredObject);
        }
    }
}