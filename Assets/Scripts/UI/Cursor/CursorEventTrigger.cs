using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CursorEventTrigger : MonoBehaviour, ICursorPressable, ICursorHoverable {
    [SerializeField] private UnityEvent<Player> onCursorHovered, onCursorPressed;

    public void OnCursorHovered(Player player) {
        onCursorHovered.Invoke(player);
    }
    public void OnCursorPressed(Player player) {
        onCursorPressed.Invoke(player);
    }
}