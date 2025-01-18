using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    public class PlayerPrefSetter : MonoBehaviour
    {
        [SerializeField] private string key = "defaultKey";

        // optional selectables to sync to settings
        [SerializeField] private Slider slider;
        [SerializeField] private Toggle toggle;

        void Start()
        {
            Sync();
        }

        public void SetPrefFloat(float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public void SetPrefInt(int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public void SetPrefBool(bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public void Sync()
        {
            if (slider != null)
            {
                if (PlayerPrefs.HasKey(key)) 
                    slider.value = PlayerPrefs.GetFloat(key);
                else
                    PlayerPrefs.SetFloat(key, slider.value);

            }
            if (toggle != null) 
            {
                if (PlayerPrefs.HasKey(key))
                    toggle.isOn = PlayerPrefs.GetInt(key) == 1;
                else
                    PlayerPrefs.SetInt(key, toggle.isOn ? 1 : 0);
            }
        }

    }
}
