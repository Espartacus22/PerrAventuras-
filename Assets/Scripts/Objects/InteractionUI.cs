using UnityEngine;
using TMPro;
public class InteractionUI
{
    private static TextMeshProUGUI text;

    public static void Show(string message)
    {
        if (text == null)
        {
            // Busca o crea automáticamente el texto en escena
            var go = GameObject.Find("InteractionText");
            if (go == null)
            {
                GameObject canvas = GameObject.Find("Canvas") ?? new GameObject("Canvas").AddComponent<Canvas>();
                if (!canvas.GetComponent<Canvas>()) canvas.gameObject.AddComponent<Canvas>();
                text = new GameObject("InteractionText").AddComponent<TextMeshProUGUI>();
                text.transform.SetParent(canvas.transform);
                text.rectTransform.anchoredPosition = new Vector2(0, -200);
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 28;
            }
            else text = go.GetComponent<TextMeshProUGUI>();
        }

        text.text = message;
        text.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        if (text != null) text.gameObject.SetActive(false);
    }