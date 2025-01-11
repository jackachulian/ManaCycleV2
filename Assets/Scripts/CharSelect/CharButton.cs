using UnityEngine;
using Battle;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Unity.Netcode;

public class CharButton : Button, ICursorHoverable, ICursorPressable
{
    [SerializeField] public Battler battler;
    [SerializeField] private Image charImage;
    [SerializeField] private Image gradient;

    

    /// <summary>
    /// Assigned by CharButtonList. Used to tell other clients that this character in the list was selected.
    /// </summary>
    public int index {get; set;}

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
        if (player && player.IsOwner) {
            player.selectedBattlerIndex.Value = index;
        } else if (player) {
            Debug.LogWarning("Non-owned player hovered a button");
        } else {
            Debug.LogWarning("Cursor without a player hovered a button");
        }
    }

    public void OnCursorPressed(Player player)
    {
        if (player.IsOwner) {
            player.selectedBattlerIndex.Value = index;
            player.characterChosen.Value = true;
        } else if (player) {
            Debug.LogWarning("Non-owned player pressed a button");
        } else {
            Debug.LogWarning("Cursor without a player hovered a button");
        }
    }
}
