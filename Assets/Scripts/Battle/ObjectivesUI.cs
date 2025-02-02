using System;
using UnityEngine;

public class ObjectivesUI : MonoBehaviour {
    /// <summary>
    /// The player's board that should be listened to
    /// </summary>
    [SerializeField] private Board board;
    [SerializeField] private Transform objectivesParent;
    [SerializeField] private ObjectiveItem objectiveItemPrefab;

    void OnEnable() {
        if (board.IsInitialized()) {
            CreateObjectiveList();
        } else {
            board.onInitialized += CreateObjectiveList;
        }
    }

    void OnDisable() {
        board.onInitialized -= CreateObjectiveList;
    }

    public void CreateObjectiveList() {
        foreach (Transform child in objectivesParent) {
            Destroy(child.gameObject);
        }

        Level level = GameManager.Instance.level;
        // dont show the objectives ui if not in a level
        if (!level) {
            gameObject.SetActive(false);
            return;
        }

        foreach (var objective in level.objectives.objectives) {
            var objectiveItem = Instantiate(objectiveItemPrefab, objectivesParent);
            objectiveItem.InitializeObjectiveItem(objective, board);
        }
    }
}