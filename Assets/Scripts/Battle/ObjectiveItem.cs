using LevelSystem.Objectives;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveItem : MonoBehaviour {
    [SerializeField] private TMP_Text objectiveLabel;
    [SerializeField] private RectTransform progressBarRectTransform;
    [SerializeField] private Image progressBarImage;
    [SerializeField] private Color inProgressTextColor;
    [SerializeField] private Color completedTextColor;
    [SerializeField] private float progressBarWidth = 9.575f;

    private LevelObjective objective;
    private Board board;

    public void InitializeObjectiveItem(LevelObjective objective, Board board) {
        this.objective = objective;
        this.board = board;
        
        UpdateObjectiveItem();

        objective.onUpdated += UpdateObjectiveItem;
        objective.ListenToBoard(board);
    }
    
    public void OnDisable() {
        if (objective != null && board != null) {
            objective.onUpdated -= UpdateObjectiveItem;
            objective.StopListeningToBoard(board);
        }
    }

    public void UpdateObjectiveItem() {
        objectiveLabel.text = objective.GetProgressString(board);
        
        float progress = objective.GetProgress(board);
        objectiveLabel.color = progress >= 1 ? completedTextColor : inProgressTextColor;
        progressBarImage.color = progress >= 1 ? completedTextColor : inProgressTextColor;
        progressBarRectTransform.sizeDelta = new Vector2(progressBarWidth * Mathf.Clamp01(progress), progressBarRectTransform.sizeDelta.y);
    }
}