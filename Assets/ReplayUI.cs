using UnityEngine;

public class ReplayUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.Instance.currentConnectionType == GameManager.GameConnectionType.Replay) {
            gameObject.SetActive(true);
        } else {
            gameObject.SetActive(false);
        }
    }
}
