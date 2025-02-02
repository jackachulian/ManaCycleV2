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
    [SerializeField] private EventSystem uiEventSystem;
    [SerializeField] private GameObject firstSelectedObject;
    [SerializeField] private GameObject buttonsParent;

    [Header("Buttons")]
    [SerializeField] private Button charSelectButton;
    [SerializeField] private Button levelSelectButton;

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

        bool inLevel = GameManager.Instance.level != null;
        charSelectButton.gameObject.SetActive(!inLevel);
        levelSelectButton.gameObject.SetActive(inLevel);

        // Select button
        uiEventSystem.enabled = true;
        uiEventSystem.SetSelectedGameObject(null);
        uiEventSystem.SetSelectedGameObject(firstSelectedObject);

    }

    public void HidePauseMenuUI()
    {
        uiEventSystem.enabled = false;
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

    public void OnLevelSelectPressed() 
    {
        GameManager.Instance.LeaveGame();
        AudioManager.Instance.PlaySound(pressedSFX);
        TransitionManager.Instance.TransitionToScene("StoryOverworld", "ReverseWipe");
    }

    public void OnMainMenuPressed() 
    {
        GameManager.Instance.LeaveGame();
        AudioManager.Instance.PlaySound(pressedSFX);
        TransitionManager.Instance.TransitionToScene("MainMenu", "ReverseWipe");
    }

    public void PlaySelectSound(BaseEventData eventData)
    {
        var axisEventData = eventData as AxisEventData;
        float pitch = (axisEventData != null) ? 1f + axisEventData.moveVector.y * 0.1f : 1f;
        AudioManager.Instance.PlaySound(selectSFX, pitch: pitch);
    }
}