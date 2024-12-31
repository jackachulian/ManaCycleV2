using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Menus;
using Audio;

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

        [SerializeField] private AudioClip selectSFX;
        [SerializeField] private AudioClip submitSFX;
        [SerializeField] private AudioClip backSFX;

        // Start is called before the first frame update
        void Start()
        {
            rootMenu.ButtonSelected += OnRootButtonSelected;
            rootMenu.ButtonSelected += sceneSwapper.OnButtonSelected;


            rootMenu.MenuOpened += RootMenuOpened;
            settingsMenu.MenuOpened += SettingsMenuOpened;

            rootMenu.ButtonSelected += PlaySelectSFX;
            settingsMenu.ButtonSelected += PlaySelectSFX;
            settingsMenu.MenuOpened += PlaySubmitSFX;
            settingsMenu.MenuClosed += PlayBackSFX;

            settingsMenu.gameObject.SetActive(false);
            rootMenu.CoroutineOpen();
        }

        // called when a new button in the radial menu is selected. Update visuals accordingly
        private void OnRootButtonSelected(int index, bool direction = true)
        {
            backgroundImage.materialForRendering.SetColor("_Color", Color.Lerp(menuColors[index], Color.black, 0.65f));
        }

        private void RootMenuOpened()
        {
            backgroundFader.SetAlphaTarget(0f);
            logoImage.SetTargets(new Vector2(-10, 10));
        }

        private void SettingsMenuOpened()
        {
            backgroundFader.SetAlphaTarget(0.85f);
            logoImage.SetTargets(new Vector2(750, 10));
        }

        private void PlaySelectSFX(int index, bool direction = true)
        {
            AudioManager.instance.PlaySound(selectSFX, direction ? 1.1f : 0.9f);
        }

        private void PlaySubmitSFX()
        {
            AudioManager.instance.PlaySound(submitSFX);
        }

        private void PlayBackSFX()
        {
            AudioManager.instance.PlaySound(backSFX);
        }

        public void Quit()
        {
            Debug.Log("Quiting");
            Application.Quit();
        }

    }
}

