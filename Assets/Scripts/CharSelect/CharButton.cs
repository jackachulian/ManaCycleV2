using UnityEngine;
using Battle;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharButton : MonoBehaviour, ICursorHoverable, ICursorPressable
{
    [SerializeField] public Battler battler;
    [SerializeField] private Image charImage;
    [SerializeField] private Image gradient;
    [SerializeField] private Selectable selectable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        charImage.sprite = battler.sprite;
        charImage.GetComponent<RectTransform>().anchoredPosition = battler.portraitOffset;
        gradient.material = battler.gradientMat;

        ColorBlock newColors = selectable.colors;
        newColors.highlightedColor = battler.mainColor;
        newColors.pressedColor = battler.altColor;
        selectable.colors = newColors;
    }

    public void OnCursorHovered(Player player)
    {
        CharSelector charSelector = CharSelectManager.Instance.GetCharSelector(player.boardIndex.Value);
        charSelector.SetDisplayedBattler(battler);
    }

    public void OnCursorPressed(Player player)
    {
        CharSelector charSelector = CharSelectManager.Instance.GetCharSelector(player.boardIndex.Value);
        charSelector.ConfirmCharacterChoice(battler);
        
    }
}
