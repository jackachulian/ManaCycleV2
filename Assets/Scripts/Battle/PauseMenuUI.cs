using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using Audio;

public class PauseMenuUI : MonoBehaviour {
    public BattleManager battleManager;
    [Header("Layout")]
    [SerializeField] private GameObject firstSelectedObject;
    [SerializeField] private GameObject buttonsParent;
    [Header("Rendering")]
    [SerializeField] private Camera renderTexCam;
    [SerializeField] private Canvas backgroundCanvas;
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerSnapshot pausedMixerSnapshot;
    [SerializeField] private AudioMixerSnapshot unpausedMixerSnapshot;
    [Header("Sound Effects")]
    [SerializeField] private AudioClip selectSFX;
    [SerializeField] private AudioClip pressedSFX;
    [SerializeField] private AudioClip openSFX;

 
    public bool menuShown {get; private set;} = false;

    void Awake() {
        if (!menuShown) gameObject.SetActive(false);

        for (int i = 0; i < buttonsParent.transform.childCount; i++)
        {
            // set every other button animation to be fliped
            Transform t = buttonsParent.transform.GetChild(i).GetChild(0);
            t.localScale -= new Vector3(i % 2 * 2, 0, 0);
            // flip text again so it is readable
            t.GetChild(1).localScale -= new Vector3(i % 2 * 2, 0, 0);
        }
    }


    public void ShowPauseMenuUI() {
        menuShown = true;
        gameObject.SetActive(true);
        // take snapshot for render texture
        // temporarily change the camera of the backgroud canvas so it is rendered by our render texture camera
        backgroundCanvas.worldCamera = renderTexCam;
        renderTexCam.Render();
        backgroundCanvas.worldCamera = Camera.main;

        pausedMixerSnapshot.TransitionTo(0.1f);
        AudioManager.Instance.PlaySound(openSFX);

        // Select button
        EventSystem.current.SetSelectedGameObject(firstSelectedObject);

    }

    public void HidePauseMenuUI()
    {
        unpausedMixerSnapshot.TransitionTo(0.1f);
        menuShown = false;
        gameObject.SetActive(false);
    }

    public void OnResumePressed()
    {
        battleManager.UnpauseGame();
        AudioManager.Instance.PlaySound(pressedSFX);

    }

    public void OnRematchPressed() 
    {
        battleManager.gameStartNetworkBehaviour.RematchRpc();
        AudioManager.Instance.PlaySound(pressedSFX);
    }

    public void OnCharacterSelectPressed() 
    {
        battleManager.gameStartNetworkBehaviour.GoToCharacterSelectRpc();
        AudioManager.Instance.PlaySound(pressedSFX);
    }

    public void OnMainMenuPressed() 
    {
        GameManager.Instance.LeaveGame();
        AudioManager.Instance.PlaySound(pressedSFX);
        TransitionManager.Instance.TransitionToScene("MainMenu", "ReverseWipe");
    }

    public void PlaySelectSound(BaseEventData eventData)
    {
        AudioManager.Instance.PlaySound(selectSFX, pitch: 1f + (eventData as AxisEventData).moveVector.y * 0.1f);
    }
}