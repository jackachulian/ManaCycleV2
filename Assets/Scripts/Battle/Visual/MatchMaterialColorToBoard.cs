using UnityEngine;

public class MatchMaterialColorToBoard : MonoBehaviour
{
    [SerializeField] private int boardIndex = 0;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private string propName = "_Color";
    [SerializeField] private float fadeTime = 0.5f;
    private Color lastCol;
    private Color targCol;
    private float refTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BattleManager.Instance.BattleInitializedNotifier += OnBattleInitialized;
        lastCol = sr.material.GetColor(propName);
    }

    private void OnBattleInitialized()
    {
        BattleManager.Instance.GetBoardByIndex(boardIndex).spellcastManager.CycleChangedNotifier += OnCycleChange;
        OnCycleChange(0);
    }

    // set tint property of this material to innerColor property of mana shader
    private void OnCycleChange(int cycleIndex)
    {
        lastCol = sr.material.GetColor(propName);
        Color col = BattleManager.Instance.cosmetics.manaVisuals[BattleManager.Instance.manaCycle.GetSequenceColor(cycleIndex)].material.GetColor("_InnerColor");
        targCol = col;
        refTime = Time.time;
    }

    void Update()
    {
        sr.material.SetColor(propName, Color.Lerp(lastCol, targCol, (Time.time - refTime) / fadeTime));
    }
}
