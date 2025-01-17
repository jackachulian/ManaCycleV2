using UnityEngine;

public class MatchMaterialColorToRenderer : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private string propertyToAnimate = "_Color";
    [SerializeField] private float fadeTime = 0.5f;
    private Renderer spriteRenderer;
    private Color lastCol;
    private Color targCol;
    private float refTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        spriteRenderer = GetComponent<Renderer>();
        if (board.IsInitialized())
        {
            OnBoardInitialized();
        } else
        {
            board.onInitialized += OnBoardInitialized;
        }
        lastCol = spriteRenderer.material.GetColor(propertyToAnimate);
    }

    private void OnBoardInitialized()
    {
        board.spellcastManager.onCycleIndexChanged += OnCycleChange;
        OnCycleChange(0);
    }

    // set tint property of this material to innerColor property of mana shader
    private void OnCycleChange(int cycleIndex)
    {
        lastCol = spriteRenderer.material.GetColor(propertyToAnimate);
        ManaVisual manaVisual = BattleManager.Instance.cosmetics.manaVisuals[board.GetManaCycle().GetSequenceColor(cycleIndex)];
        Color col = manaVisual.material.GetColor("_InnerColor");
        targCol = col;
        refTime = Time.time;
    }

    void Update()
    {
        spriteRenderer.material.SetColor(propertyToAnimate, Color.Lerp(lastCol, targCol, (Time.time - refTime) / fadeTime));
    }
}
