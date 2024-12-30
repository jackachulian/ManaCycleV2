using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace MainMenu
{
    // ties multiple parts of the main menu together
    public class MainMenuController : MonoBehaviour
    {
        // should correspond to the color of each button
        [SerializeField] private Color[] menuColors;
        [SerializeField] private HalfRadialButtons rootMenu;
        [SerializeField] private HalfRadialButtons settingsMenu;
        [SerializeField] private SelectionSceneSwapper sceneSwapper;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private ImageColorSmoother backgroundFader;
        [SerializeField] private RectTransformSmoother logoImage;

        // Start is called before the first frame update
        void Start()
        {
            rootMenu.ButtonSelected += OnButtonSelected;
            rootMenu.ButtonSelected += sceneSwapper.OnButtonSelected;

            rootMenu.MenuOpened += RootMenuOpened;
            settingsMenu.MenuOpened += SettingsMenuOpened;

            settingsMenu.gameObject.SetActive(false);
            rootMenu.CoroutineOpen();
        }

        // called when a new button in the radial menu is selected. Update visuals accordingly
        void OnButtonSelected(int index, bool direction = true)
        {
            backgroundImage.materialForRendering.SetColor("_Color", Color.Lerp(menuColors[index], Color.black, 0.65f));
        }

        void RootMenuOpened()
        {
            backgroundFader.SetAlphaTarget(0f);
            logoImage.SetTargets(new Vector2(-10, 10));
        }

        void SettingsMenuOpened()
        {
            backgroundFader.SetAlphaTarget(0.85f);
            logoImage.SetTargets(new Vector2(750, 10));
        }

        public void Quit()
        {
            Debug.Log("Quiting");
            Application.Quit();
        }

        public void OpenQuitMenu()
        {
            backgroundFader.SetAlphaTarget(0.85f);
            logoImage.SetTargets(new Vector2(750, 10));
            rootMenu.CoroutineClose();
        }

    }
}

