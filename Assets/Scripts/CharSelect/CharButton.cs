using UnityEngine;
using Battle;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class CharButton : Button, ICursorHoverable, ICursorPressable
{
    [SerializeField] public Battler battler;
    [SerializeField] private Image charImage;
    [SerializeField] private Image gradient;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        charImage.sprite = battler.sprite;
        charImage.GetComponent<RectTransform>().anchoredPosition = battler.portraitOffset;
        gradient.material = battler.gradientMat;

        ColorBlock newColors = colors;
        newColors.highlightedColor = battler.mainColor;
        newColors.pressedColor = battler.altColor;
        colors = newColors;
    }

    public void OnCursorHovered(Player player)
    {
        if (player) {
            CharSelector charSelector = CharSelectManager.Instance.GetCharSelector(player.boardIndex.Value);
            charSelector.SetDisplayedBattler(battler);
        }
    }

    public void OnCursorPressed(Player player)
    {
        if (player) {
            CharSelector charSelector = CharSelectManager.Instance.GetCharSelector(player.boardIndex.Value);
            if (charSelector.state == CharSelector.CharSelectorState.ChoosingCharacter) {
                charSelector.ConfirmCharacterChoice(battler);
            } else if (charSelector.state == CharSelector.CharSelectorState.Options) {
                // if the character the player chose before the options menu was clicked, confirm the options
                if (charSelector.selectedBattler == battler) {
                    charSelector.ConfirmOptions();
                } 
                // otherwise, dont do anything
            }
        }
    }

    /// <summary>
    /// Allows mouse clicks to also interact with this object.
    /// </summary>
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        var player = eventData.currentInputModule.gameObject.GetComponent<Player>();
        
        OnCursorPressed(player);
    }
}
