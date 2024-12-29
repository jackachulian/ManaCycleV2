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
        private float radius;
        // note that angles between 180 and 270 will be upside down due to rotation 
        [SerializeField] private float minAng;
        [SerializeField] private float maxAng;
        // amount of selectables that are shown at once
        [SerializeField] private int shownAmount;
        [SerializeField] private float openRadius;
        [SerializeField] private float closeRadius;
        // currently selected button
        private int currentSelectionIndex = 0;
        // if this menu needs to scroll to show all buttons. set in start
        private bool shouldScroll;
        // if this menu saves your last selected item when re-opening
        [SerializeField] private bool savePosition = true;

        public delegate void ButtonSelectedHandler(int index, bool direction);
        // called when the selected button is changed.
        public event ButtonSelectedHandler ButtonSelected;

        public delegate void ButtonSubmittedHandler(int index);
        public event ButtonSubmittedHandler ButtonSubmitted;

        // Start is called before the first frame update
        void Start()
        {
            radius = openRadius;
            shouldScroll = items.Length - shownAmount >= 2;
            itemTransforms = new RectTransformSmoother[items.Length];
            // set initial visibility of selectable gameobjects to false for simplicity
            for (int i = 0; i < items.Length; i++)
            {
                itemTransforms[i] = items[i].GetComponent<RectTransformSmoother>();
                items[i].gameObject.SetActive(false);
            }

            ShowVisibleButtons();

            EventSystem.current.SetSelectedGameObject(items[currentSelectionIndex].gameObject);
        }

        void ShowVisibleButtons(float smoothTime = 0f)
        {
            // show only visible buttons
            int start = shouldScroll ?  -shownAmount / 2 : 0;
            int end = shouldScroll ? shownAmount / 2 + shownAmount % 2 : items.Length;
            for (int i = start; i < end; i++)
            {
                int s = Mod(i + (shouldScroll ? currentSelectionIndex : 0), items.Length);
                items[s].gameObject.SetActive(true);
                // set initial positions
                float theta = ToTheta(i);
                itemTransforms[s].SetTargets(
                    new Vector3(Mathf.Cos(theta) * radius, Mathf.Sin(theta) * radius, 0), 
                    new Vector3(0, 0, theta * Mathf.Rad2Deg),
                    st: smoothTime
                );
            }
        }

        // animate buttons rotating
        private void RotateItems(int selectionDelta, int endDelta)
        {
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
        }

        private void SelectNext(int selectionDelta = 1)
        {
            // difference between current index and bottommost visible item index
            int endDelta = (shownAmount / 2 + 1) * (int) Mathf.Sign(selectionDelta);

            if (shouldScroll) RotateItems(selectionDelta, endDelta);

            // change selected button index and raise event that change happend for other visuals to update
            currentSelectionIndex = Mod(currentSelectionIndex + selectionDelta, items.Length);
            SelectItem(selectionDelta);

        }

        private void SelectItem(int selectionDelta = 1)
        {
            ButtonSelected?.Invoke(currentSelectionIndex, selectionDelta < 0);
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
            // TEMP replace with new input system
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SelectNext(-1);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SelectNext(1);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ButtonSubmitted?.Invoke(currentSelectionIndex);
            }
        }

        public void TransitionRadius(float r, float st = 0.1f)
        {
            radius = r;
            ShowVisibleButtons(st);
        }

        public IEnumerator Close()
        {
            // transition out before closing
            TransitionRadius(closeRadius);
            // wait for transition time before deactivating
            // TODO CLEANUP don't hardcode 0.1f, maybe use callback on transition complete
            yield return new WaitForSeconds(0.1f);
            gameObject.SetActive(false);

        }

        public IEnumerator Open()
        {
            if (!savePosition) currentSelectionIndex = 0;

            TransitionRadius(closeRadius, 0f);
            gameObject.SetActive(true);
            TransitionRadius(openRadius);
            yield return new WaitForSeconds(0.1f);

            SelectItem();
        }

        // used in event triggers
        public void CoroutineOpen()
        {
            gameObject.SetActive(true);
            StartCoroutine(Open());
        }

        public void CoroutineClose()
        {
            StartCoroutine(Close());
        }
    }
}
