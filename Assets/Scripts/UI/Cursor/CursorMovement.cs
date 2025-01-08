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
    void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        rt.anchoredPosition += inputVector * cursorSpeed * Time.deltaTime;
    }

    public void SetCursorMovement(Vector2 inputVector)
    {
        this.inputVector = inputVector;
    }
}
