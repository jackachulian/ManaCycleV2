using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Menus;

namespace MainMenu
{
    public class SelectionSceneSwapper : MonoBehaviour
    {
        [SerializeField] RectTransformSmoother[] selectionScenes;
        // new scene transitions in from this position
        [SerializeField] Vector3 incomingPos;
        // old scene transitions out to this position
        [SerializeField] Vector3 outgoingPos;
        // new scene stops at this position
        [SerializeField] Vector3 primaryPos;

        private int lastIndex = 0;

        // Start is called before the first frame update
        void Start()
        {
            foreach (RectTransformSmoother o in selectionScenes)
            {
                o.gameObject.SetActive(false);
            }
        }

        // quick hack for demonstration
        public void OnButtonSelected(int index, bool direction)
        {
            Vector3 _incomingPos = direction ? outgoingPos : incomingPos;
            Vector3 _outgoingPos = direction ? incomingPos : outgoingPos;
            // hide offscreen scene
            selectionScenes[lastIndex].SetTargets(_outgoingPos);

            selectionScenes[index].SetImmediate(_incomingPos);
            selectionScenes[index].gameObject.SetActive(true);
            selectionScenes[index].SetTargets(primaryPos);

            lastIndex = index;
        }
    }
}
