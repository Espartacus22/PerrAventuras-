using UnityEngine;

public class Crosshair : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("Estado")]
    public bool useMouseCursorPosition = true;
    public bool lockToScreenCenterWhenHidden = true;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (rectTransform == null) return;

        // Si el cursor está visible, la mirilla sigue al mouse
        if (Cursor.visible && useMouseCursorPosition)
        {
            rectTransform.position = Input.mousePosition;
        }
        // Si el cursor está oculto/bloqueado, la mirilla queda al centro
        else if (lockToScreenCenterWhenHidden)
        {
            rectTransform.position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }
    }
}
