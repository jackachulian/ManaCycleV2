using UnityEngine;
using UnityEngine.UI;
using Audio;

namespace Menus
{
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private HalfRadialButtons settingsMenu;
        [SerializeField] private GameObject[] subMenus;

        [SerializeField] private AudioClip sliderSFX;
        [SerializeField] private AudioClip returnSFX;
        [SerializeField] private AudioClip selectionSFX;
        [SerializeField] private AudioClip specialSFX;
        // first item to select in each submenu
        private int lastIndex = -1;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            settingsMenu.ButtonSelected += OnButtonSelected;
            foreach (GameObject o in subMenus) o.SetActive(false);
        }

        void OnButtonSelected(int index, bool direction = true)
        {
            if (lastIndex >= 0) subMenus[lastIndex].SetActive(false);
            if (index < subMenus.Length) 
            {
                subMenus[index].gameObject.SetActive(true);
                lastIndex = index;
            }
        }

        public void PlaySliderSound(bool special)
        {
            AudioManager.instance.PlaySound(special ? specialSFX : sliderSFX);
        }

        public void PlaySelectionSound()
        {
            AudioManager.instance.PlaySound(selectionSFX);   
        }

        public void PlayReturnSound()
        {
            AudioManager.instance.PlaySound(returnSFX);
        }

        public void VolumeSliderChanged()
        {
            AudioManager.instance.UpdateVolumes();
        }
    }   
}

