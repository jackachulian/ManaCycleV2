using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Battle;
using UnityEngine.InputSystem.UI;

public class CharPortriat : MonoBehaviour
{
    [SerializeField] private Image battlerPortrait;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] string defaultText;
    [SerializeField] string selectText;
    [SerializeField] public GameObject optionsWindow;
    [SerializeField] private GameObject readyWindow;
    [SerializeField] public GameObject firstOption;
    private RectTransform portriatRectTransform;
    private Vector2 defaultPos;

    public bool ready;

    public delegate void ReadyStateChangedHandler(bool ready);
    public event ReadyStateChangedHandler ReadyStateChanged;

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

    public void SetSelectText()
    {
        nameText.text = selectText;
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

    public void OpenOptions(PlayerCursorController cursor)
    {
        optionsWindow.SetActive(true);
        cursor.GetComponent<MultiplayerEventSystem>()
            .SetSelectedGameObject(firstOption);
        cursor.GetComponent<MultiplayerEventSystem>().enabled = true;
        
    }

    public void CloseOptions(PlayerCursorController cursor)
    {
        SetReady(false);
        optionsWindow.SetActive(false);
        cursor.GetComponent<MultiplayerEventSystem>()
            .SetSelectedGameObject(null);
        cursor.GetComponent<MultiplayerEventSystem>().enabled = false;
    }

    // after options are selected
    public void SetReady(bool ready)
    {
        this.ready = ready;
        ReadyStateChanged?.Invoke(ready);
        optionsWindow.SetActive(!ready);
        readyWindow.SetActive(ready);
    }
}
