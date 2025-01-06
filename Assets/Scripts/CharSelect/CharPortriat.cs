using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Battle;

public class CharPortriat : MonoBehaviour
{
    [SerializeField] Image battlerPortrait;
    [SerializeField] Image background;
    [SerializeField] TMP_Text nameText;
    [SerializeField] string defaultText;
    private RectTransform portriatRectTransform;
    private Vector2 defaultPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetDefault();
    }

    void Awake()
    {
        portriatRectTransform = battlerPortrait.GetComponent<RectTransform>();
        defaultPos = portriatRectTransform.anchoredPosition;
    }

    public void SetDefault()
    {
        battlerPortrait.sprite = null;
        battlerPortrait.color = new Color(1f, 1f, 1f, 0f);
        nameText.text = defaultText;
    }

    public void SetBattler(Battler battler)
    {
        battlerPortrait.sprite = battler.sprite;
        battlerPortrait.color = new Color(1f, 1f, 1f, 1f);
        portriatRectTransform.anchoredPosition = defaultPos + battler.portraitOffset;
        nameText.text = battler.displayName;
    }

    public void SetLocked(bool locked)
    {
        battlerPortrait.color = new Color(1f, 1f, 1f, locked ? 1f : 0.5f);
    }
}
