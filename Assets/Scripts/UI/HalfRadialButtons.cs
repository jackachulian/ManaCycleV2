using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace MainMenu
{
    public class HalfRadialButtons : MonoBehaviour
    {
        // list of buttons
        [SerializeField] private Selectable[] items;
        // generated from list of buttons
        private RectTransformSmoother[] itemTransforms;
        [SerializeField] private float radius;
        // note that angles between 180 and 270 will be upside down due to rotation 
        [SerializeField] private float minAng;
        [SerializeField] private float maxAng;
        // amount of selectables that are shown at once
        [SerializeField] private int shownAmount;
        // currently selected button
        private int currentSelectionIndex = 0;

        public delegate void ButtonSelectedHandler(int index, bool direction);
        // called when the selected button is changed.
        public event ButtonSelectedHandler ButtonSelected;

        // Start is called before the first frame update
        void Start()
        {
            // Time.timeScale = 0.05f;
            itemTransforms = new RectTransformSmoother[items.Length];
            // set initial visibility of selectable gameobjects to false for simplicity
            for (int i = 0; i < items.Length; i++)
            {
                itemTransforms[i] = items[i].GetComponent<RectTransformSmoother>();
                items[i].gameObject.SetActive(false);
            }

            // show only visible buttons
            for (int i = -shownAmount / 2; i < shownAmount / 2 + shownAmount % 2; i++)
            {
                int s = Mod(i + currentSelectionIndex, items.Length);
                items[s].gameObject.SetActive(true);
                // set initial positions
                float theta = ToTheta(i);
                itemTransforms[s].SetImmediate(
                    new Vector3(Mathf.Cos(theta) * radius, Mathf.Sin(theta) * radius, 0), 
                    new Vector3(0, 0, theta * Mathf.Rad2Deg)
                );
            }

            EventSystem.current.SetSelectedGameObject(items[currentSelectionIndex].gameObject);
        }

        void SelectNext(int selectionDelta = 1)
        {
            // difference between current index and bottommost visible item index
            int endDelta = (shownAmount / 2 + 1) * (int) Mathf.Sign(selectionDelta);

            // deactive the item that is now off the top of the screen
            items[Mod(currentSelectionIndex - endDelta, items.Length)].gameObject.SetActive(false);

            // move last visible element to bottommost offscreen position for smooth transition
            float offscreenTheta = ToTheta(endDelta);
            itemTransforms[Mod(currentSelectionIndex + endDelta, items.Length)].SetImmediate(
                    new Vector3(Mathf.Cos(offscreenTheta) * radius, Mathf.Sin(offscreenTheta) * radius, 0), 
                    new Vector3(0, 0, offscreenTheta * Mathf.Rad2Deg)
            );

            // show item that is about to come up from the bottom of the screen
            items[Mod(currentSelectionIndex + endDelta, items.Length)].gameObject.SetActive(true);
            // itemTransforms[Mod(currentSelectionIndex + shownAmount / 2 + 1, items.Length)].JumpImmediateToTarget();


            // rotate all buttons up
            for (int i = -shownAmount / 2 - 1; i < shownAmount / 2 + shownAmount % 2 + 1; i++)
            {
                int s = Mod(i + currentSelectionIndex, items.Length);
                float theta = ToTheta(i - 1 * Math.Sign(selectionDelta));
                itemTransforms[s].SetTargets(
                    new Vector3(Mathf.Cos(theta) * radius, Mathf.Sin(theta) * radius, 0), 
                    new Vector3(0, 0, theta * Mathf.Rad2Deg)
                );
            }

            // change selected button index and raise event that change happend for other visuals to update
            currentSelectionIndex = Mod(currentSelectionIndex + selectionDelta, items.Length);
            ButtonSelected.Invoke(currentSelectionIndex, selectionDelta < 0);
            EventSystem.current.SetSelectedGameObject(items[currentSelectionIndex].gameObject);

        }

        private float ToTheta(int i)
        {
            return ((float) (i + 2 / shownAmount) / (float) shownAmount * (maxAng - minAng) + minAng) * Mathf.Deg2Rad;
        }

        // TEMP to be moved to utils
        private int Mod(int n, int m)
        {
            return (n % m + m) % m; 
        }

        // Update is called once per frame
        void Update()
        {
            // TEMP
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SelectNext(-1);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SelectNext(1);
            }
        }
    }
}
