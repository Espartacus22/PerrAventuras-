using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public RectTransform leftDot;
    public RectTransform rightDot;
    public float expandAmount = 10f;
    public float expandSpeed = 5f;

    private float currentExpand = 0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            currentExpand = expandAmount;

        currentExpand = Mathf.Lerp(currentExpand, 0, Time.deltaTime * expandSpeed);

        leftDot.anchoredPosition = new Vector2(-15 - currentExpand, 0);
        rightDot.anchoredPosition = new Vector2(15 + currentExpand, 0);
    }
}
