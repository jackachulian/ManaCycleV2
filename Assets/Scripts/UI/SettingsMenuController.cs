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

        //
        public void PlaySelectionSound()
        {
            AudioManager.instance.PlaySound(sliderSFX);   
        }

        public void VolumeSliderChanged()
        {
            AudioManager.instance.UpdateVolumes();
        }
    }   
}

