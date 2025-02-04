using StoryMode.Overworld;
using UnityEngine;

public class LevelPopup : MonoBehaviour {
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private RectTransform uiTransform;
    [SerializeField] private Vector2 popupOffset;

    [SerializeField] private TMPro.TMP_Text levelNameLabel;
    [SerializeField] private TMPro.TMP_Text levelNumberLabel;

    [SerializeField] private CanvasGroup popupCanvasGroup;

    private LevelInteractable currentLevelInteractable;

    private bool showNearbyLevels = true;

    void Start() {
        HideUI();
        interactionManager.onNearestChanged += OnInteractionManagerNearestChanged;
    }

    void Update() {
        UpdatePopupPosition();
    }

    void UpdatePopupPosition() {
        if (currentLevelInteractable) {
            var screenPos = (Vector2)Camera.main.WorldToScreenPoint(currentLevelInteractable.transform.position) - new Vector2(Screen.width, Screen.height)/2;
            uiTransform.anchoredPosition = screenPos + popupOffset;
        }
    }

    void OnInteractionManagerNearestChanged(OverworldInteractable nearest) {
        if (!showNearbyLevels) return;

        LevelInteractable levelInteractable = nearest as LevelInteractable;
        if (levelInteractable) {
            if (levelInteractable.level) {
                currentLevelInteractable = levelInteractable;
                DisplayLevel(levelInteractable.level);
                ShowUI();
            }
        } else {
            currentLevelInteractable = null;
            HideUI();
        }
    }

    public void ShowUI() {
        // TODO: maybe use an animator instead of showing/hiding object instantly
        // (morgan you can do this if you want)
        uiTransform.gameObject.SetActive(true);
        UpdatePopupPosition();
    }

    public void HideUI() {
        if (uiTransform) uiTransform.gameObject.SetActive(false);
    }

    public void DisplayLevel(Level level) {
        if (!level) {
            Debug.LogError("Trying to display a null level");
            return;
        }

        levelNameLabel.text = level.displayName;
        levelNumberLabel.text = level.levelNumber;
    }

    public void StopShowingNearbyLevels() {
        showNearbyLevels = false;
        HideUI();
    }

    public void ShowNearbyLevels() {
        showNearbyLevels = true;
        OnInteractionManagerNearestChanged(interactionManager.nearest);
    }

    public void Dim() {
        popupCanvasGroup.alpha = 0.3f;
    }

    public void Undim() {
        popupCanvasGroup.alpha = 1f;
    }
}