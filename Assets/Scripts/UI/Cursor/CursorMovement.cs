using UnityEngine;
using UnityEngine.InputSystem;

public class CursorMovement : MonoBehaviour
{
    private RectTransform rt;
    [SerializeField] private Vector2 cursorSpeed;

    /// <summary>
    /// The current vector to move the cursor with as set by the CharSelectInputHandler
    /// </summary>
    private Vector2 inputVector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        rt.anchoredPosition += inputVector * cursorSpeed * Time.deltaTime;
        rt.position = new Vector2(Mathf.Clamp(rt.position.x, 0, Screen.width), Mathf.Clamp(rt.position.y, 0, Screen.height));
    }

    public void SetCursorMovement(Vector2 inputVector)
    {
        this.inputVector = inputVector;
    }

    public void SetPosition(Vector2 position) {
        if (!rt) rt = GetComponent<RectTransform>();
        rt.position = new Vector2(Mathf.Clamp(position.x, 0, Screen.width), Mathf.Clamp(position.y, 0, Screen.height));
    }
}
