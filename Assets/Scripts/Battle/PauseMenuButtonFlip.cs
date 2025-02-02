using UnityEngine;

public class PauseMenuButtonFlip : MonoBehaviour {
    [SerializeField] bool flipped = false;
    [SerializeField] private Transform textTransform;

    void Start() {
        if (flipped) {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

            // flip text again so it is readable
            textTransform.localScale = new Vector3(textTransform.localScale.x * -1, textTransform.localScale.y, textTransform.localScale.z);
        }
    }
}