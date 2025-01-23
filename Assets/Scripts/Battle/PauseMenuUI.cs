using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenuUI : MonoBehaviour {
    public BattleManager battleManager;
    [SerializeField] private GameObject firstSelectedObject;
    [SerializeField] private GameObject buttonsParent;

    public bool menuShown {get; private set;} = false;

    void Awake() {
        if (!menuShown) gameObject.SetActive(false);

        // set every other button animation to be fliped
        for (int i = 0; i < buttonsParent.transform.childCount; i++)
        {
            // flip button
            Transform t = buttonsParent.transform.GetChild(i);
            t.localScale -= new Vector3(i % 2 * 2, 0, 0);
            // flip text again so it is readable
            t.GetChild(1).localScale -= new Vector3(i % 2 * 2, 0, 0);
        }
    }


    public void ShowPauseMenuUI() {
        menuShown = true;
        gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstSelectedObject);

    }

    public void HidePauseMenuUI()
    {
        menuShown = false;
        gameObject.SetActive(false);
    }

    public void OnResumePressed()
    {
        battleManager.UnpauseGame();
    }

    public void OnRematchPressed() {
        battleManager.gameStartNetworkBehaviour.RematchRpc();
    }

    public void OnCharacterSelectPressed() {
        battleManager.gameStartNetworkBehaviour.GoToCharacterSelectRpc();
    }

    public void OnMainMenuPressed() {
        GameManager.Instance.LeaveGame();
        TransitionManager.Instance.TransitionToScene("MainMenu", "ReverseWipe");
    }

    public void OpenAnimationComplete()
    {
        EventSystem.current.SetSelectedGameObject(firstSelectedObject);
    }
}