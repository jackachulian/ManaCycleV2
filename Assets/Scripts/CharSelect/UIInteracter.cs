using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Linq;

public class UIInteracter : MonoBehaviour
{
    private GraphicRaycaster graphicRaycaster;
    private bool submitPressed;
    private List<GameObject> lastFrameResults = new();
    public int playerNum;

    public delegate void OnCursorInteractionHandler(int playerNum, GameObject interacted);
    public event OnCursorInteractionHandler CursorSubmit;
    public event OnCursorInteractionHandler CursorHover;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // place self under canvas if spawned by input manager
        Transform canvas = GameObject.Find("Canvas").transform;
        transform.SetParent(canvas, false);
        graphicRaycaster = GetComponentInParent<GraphicRaycaster>();

        playerNum = GameObject.FindObjectsByType<PlayerCursorMovement>(FindObjectsSortMode.None).Count() - 1;
        Debug.Log("Player" + playerNum + " join");
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
            CursorSubmit?.Invoke(playerNum, r);
        });
        
        lastFrameResults.Where(r => !results.Contains(r)).ToList().ForEach(r => 
        {
            ExecuteEvents.Execute(r, pointer, ExecuteEvents.pointerExitHandler);
        });
        
        results.Where(r => !lastFrameResults.Contains(r)).ToList().ForEach(r => 
        {
            ExecuteEvents.Execute(r, pointer, ExecuteEvents.pointerEnterHandler);
            CursorHover?.Invoke(playerNum, r);
        });

        submitPressed = false;
        lastFrameResults = new List<GameObject>(results);
    }

    void OnSubmit(InputValue value)
    {
        submitPressed = value.isPressed;
    }
}
