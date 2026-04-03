using UnityEngine;

public class Crosshair : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (rectTransform == null) return;
        rectTransform.position = Input.mousePosition;
    }
}
