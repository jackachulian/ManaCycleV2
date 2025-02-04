using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ShowableMenu that simply activates and deactivates a gameobject to show and hide the menu.
/// </summary>
public class SimpleShowableMenu : ShowableMenu
{
    [SerializeField] private GameObject menuObject;
    [SerializeField] private GameObject firstSelected;
    [SerializeField] private bool dimWhileUncontrolled = true;
    [SerializeField] private float uncontrolledAlpha = 0.5f;

    private CanvasGroup canvasGroup;

    protected override void OnEnable() {
        base.OnEnable();
        canvasGroup = GetComponent<CanvasGroup>();

        menuObject.SetActive(false);

        onShow += OnShow;
        onHide += OnHide;
        onControlEnter += OnControlEnter;
        onControlExit += OnControlExit;
    }

    protected override void OnDisable() {
        base.OnDisable();

        onShow -= OnShow;
        onHide -= OnHide;
        onControlEnter -= OnControlEnter;
        onControlExit -= OnControlExit;
    }

    public void OnShow() {
        menuObject.SetActive(true);
    }

    public void OnHide() {
        menuObject.SetActive(false);
    }

    public void OnControlEnter()
    {
        if (dimWhileUncontrolled && canvasGroup) canvasGroup.alpha = 1;
        if (firstSelected) {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
        
    }

    public virtual void OnControlExit()
    {
        if (dimWhileUncontrolled && canvasGroup) canvasGroup.alpha = uncontrolledAlpha;
    }    
}