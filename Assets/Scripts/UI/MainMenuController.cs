using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        // should correspond to the color of each button
        [SerializeField] private Color[] menuColors;
        [SerializeField] private HalfRadialButtons rootMenu;
        [SerializeField] private HalfRadialButtons settingsMenu;
        [SerializeField] private SelectionSceneSwapper sceneSwapper;
        [SerializeField] private Image backgroundImage;

        // Start is called before the first frame update
        void Start()
        {
            rootMenu.ButtonSelected += OnButtonSelected;
            rootMenu.ButtonSelected += sceneSwapper.OnButtonSelected;

            settingsMenu.gameObject.SetActive(false);
            rootMenu.CoroutineOpen();
            
            // TODO cleanup radial button menu init
            // OnButtonSelected(0);
            sceneSwapper.OnButtonSelected(0, true);
        }

        // called when a new button in the radial menu is selected. Update visuals accordingly
        void OnButtonSelected(int index, bool direction = true)
        {
            backgroundImage.materialForRendering.SetColor("_Color", Color.Lerp(menuColors[index], Color.black, 0.65f));
        }

    }
}

