using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Linq;

public class CursorUIInteractor : MonoBehaviour
{
    private GraphicRaycaster graphicRaycaster;
    private bool submitPressed;
    private List<GameObject> lastFrameResults = new();

    // Player that can be associated with inputs.
    private Player player;

    // public delegate void OnCursorInteractionHandler(int playerNum, GameObject interacted);
    // public event OnCursorInteractionHandler OnCursorSubmit;
    // public event OnCursorInteractionHandler OnCursorHover;

    // public delegate void OnCursorReturnHandler(int playerNum);
    // public event OnCursorReturnHandler OnCursorReturn;

    // called by menu
    public void Initialize()
    {
        // place self under canvas if spawned by input manager
        Transform canvas = GameObject.Find("Canvas").transform;
        transform.SetParent(canvas, false);
        graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
    }

    /// <summary>
    /// Make it so clicks from this cursor are associated with a player ID.
    /// </summary>
    public void SetPlayer(Player player) {
        this.player = player;
    }

    // Update is called once per frame
    void Update()
    {
        PointerEventData pointer = new (EventSystem.current) {position = transform.position};

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointer, raycastResults);

        List<GameObject> results = new();
        raycastResults.Where(r => r.gameObject.GetComponent<Selectable>()).ToList().ForEach(r => results.Add(r.gameObject));

        if (submitPressed) results.ForEach(r => 
        {
            ExecuteEvents.Execute(r, pointer, ExecuteEvents.submitHandler);
            // OnCursorSubmit?.Invoke(playerNum, r);
            
            ICursorPressable pressable = r.GetComponent<ICursorPressable>();
            if (pressable != null) {
                pressable.OnCursorPressed(player);
            }
        });
        
        lastFrameResults.Where(r => !results.Contains(r)).ToList().ForEach(r => 
        {
            ExecuteEvents.Execute(r, pointer, ExecuteEvents.pointerExitHandler);
        });
        
        results.Where(r => !lastFrameResults.Contains(r)).ToList().ForEach(r => 
        {
            ExecuteEvents.Execute(r, pointer, ExecuteEvents.pointerEnterHandler);
            
            ICursorHoverable pressable = r.GetComponent<ICursorHoverable>();
            if (pressable != null) {
                pressable.OnCursorHovered(player);
            }
        });

        submitPressed = false;
        lastFrameResults = new List<GameObject>(results);
    }

    /// <summary>
    /// Send a submit event to the hovered object(s?).
    /// </summary>
    public void Submit()
    {
        submitPressed = true;
    }

    public void Cancel()
    {
        
    }
}
