using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    private float currentTime = 0f;
    private float bestTime = float.MaxValue;
    private int deathCount = 0;
    private bool timing = true;

    private TextMeshProUGUI timerText;
    private TextMeshProUGUI bestTimeText;
    private TextMeshProUGUI deathText;

    void Awake()
    {
        Instance = this;
        BuildUI();
    }

    void Update()
    {
        if (timing)
        {
            currentTime += Time.deltaTime;
            UpdateUI();
        }
    }

    public void OnDeath()
    {
        timing = true; // restart timing on death even after finish
        deathCount++;
        if (currentTime < bestTime)
            bestTime = currentTime;
        currentTime = 0f;
        UpdateUI();
    }

    private void UpdateUI()
    {
        timerText.text = "Time: " + FormatTime(currentTime);
        bestTimeText.text = bestTime == float.MaxValue ? "Best: --" : "Best: " + FormatTime(bestTime);
        deathText.text = "Deaths: " + deathCount;
    }

    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time * 100) % 100);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    private void BuildUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Timer (top right)
        timerText = CreateText(canvasObj, "TimerText", new Vector2(-20, -20),
            new Vector2(1, 1), new Vector2(1, 1));

        // Best time (below timer)
        bestTimeText = CreateText(canvasObj, "BestTimeText", new Vector2(-20, -60),
            new Vector2(1, 1), new Vector2(1, 1));

        // Deaths (below best time)
        deathText = CreateText(canvasObj, "DeathText", new Vector2(-20, -100),
            new Vector2(1, 1), new Vector2(1, 1));

        UpdateUI();
    }

    private TextMeshProUGUI CreateText(GameObject parent, string name, Vector2 position, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopRight;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 40);
        return tmp;
    }
    public void OnFinish()
    {
        timing = false;
        if (currentTime < bestTime)
            bestTime = currentTime;
        UpdateUI();
        timerText.text = "Time: " + FormatTime(currentTime) + " DONE!";
    }
}