using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    void Start()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("CrosshairCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create crosshair image
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(canvasObj.transform);
        Image img = crosshairObj.AddComponent<Image>();
        img.color = Color.white;

        // Center it and size it
        RectTransform rect = crosshairObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(10, 10);
    }
}