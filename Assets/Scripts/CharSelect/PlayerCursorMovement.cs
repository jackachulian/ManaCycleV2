using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCursorMovement : MonoBehaviour
{
    private RectTransform rt;
    [SerializeField] private Vector2 cursorSpeed;
    private Vector2 posDelta;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        rt.anchoredPosition += posDelta * Time.deltaTime;
    }

    public void OnNavigate(InputValue value)
    {
        Vector2 v = value.Get<Vector2>();
        posDelta = v.normalized * cursorSpeed;
    }
}
