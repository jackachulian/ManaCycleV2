using UnityEngine;

/// <summary>
/// Represents a layout that can be selected by the board layout manager. Contains a certain amount of layouts.
/// </summary>
public class BoardLayout : MonoBehaviour {
    /// <summary>
    /// All boards in the layout. Minimum 1, max 4
    /// </summary>
    [SerializeField] private Board[] _boards;
    public Board[] boards => _boards;

    
    /// <summary>
    /// The Mana Cycle object that dictates the order of color clears.
    /// </summary>
    [SerializeField] private ManaCycle _manaCycle;
    public ManaCycle manaCycle => _manaCycle;

    

    public void ShowLayout() {
        gameObject.SetActive(true);
    }

    public void HideLayout() {
        gameObject.SetActive(false);
    }
}