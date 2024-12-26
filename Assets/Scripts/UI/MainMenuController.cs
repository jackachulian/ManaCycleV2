using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        // should correspond to the color of each button
        [SerializeField] private Color[] menuColors;
        [SerializeField] private HalfRadialButtons radialMenu;
        [SerializeField] private SelectionSceneSwapper sceneSwapper;
        [SerializeField] private Image backgroundImage;

        // Start is called before the first frame update
        void Start()
        {
            radialMenu.ButtonSelected += OnButtonSelected;
            radialMenu.ButtonSelected += sceneSwapper.OnButtonSelected;
            
            // TODO cleanup radial button menu init
            OnButtonSelected(0);
            sceneSwapper.OnButtonSelected(0, true);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        // called when a new button in the radial menu is selected. Update visuals accordingly
        void OnButtonSelected(int index, bool direction = true)
        {
            backgroundImage.materialForRendering.SetColor("_Color", Color.Lerp(menuColors[index], Color.black, 0.65f));
        }
    }
}

