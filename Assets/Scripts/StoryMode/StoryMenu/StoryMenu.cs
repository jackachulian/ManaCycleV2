using System.Threading.Tasks;
using StoryMode.Overworld;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class StoryMenu : SimpleShowableMenu {
    [SerializeField] private MenuPanelSwapper _menuPanelSwapper;
    public MenuPanelSwapper menuPanelSwapper => _menuPanelSwapper;
}