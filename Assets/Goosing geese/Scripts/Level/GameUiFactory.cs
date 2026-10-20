using System;
using UnityEngine;
using UnityEngine.UI;

public class GameUiFactory
{
    private const float ReferenceWidth = 720f;
    private const float ReferenceHeight = 1280f;
    private const float LabelHeightMultiplier = 2.4f;
    private const float ButtonHeight = 68f;
    private const int ButtonFontSize = 25;
    private static readonly Color ButtonColor = new Color(0.12f, 0.32f, 0.36f);

    private readonly Font _font;
    private readonly Action _playClick;

    public GameUiFactory(Font font, Action playClick)
    {
        _font = font;
        _playClick = playClick;
    }

    public static RectTransform CreateRect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        Place(rect, min, max);
        return rect;
    }

    public static void Place(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public static GameObject CreateCanvas(string name, Transform parent, int sortingOrder)
    {
        var root = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(parent, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.matchWidthOrHeight = 0.5f;
        return root;
    }

    public static void UpdateSafeArea(RectTransform rect)
    {
        Rect safe = Screen.safeArea;
        rect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        rect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
    }

    public Text CreateLabel(Transform parent, string text, int size)
    {
        var rect = CreateRect("Label", parent, Vector2.zero, Vector2.one);
        var label = rect.gameObject.AddComponent<Text>();
        label.font = _font;
        label.fontSize = size;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = text;
        label.raycastTarget = false;
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = size * LabelHeightMultiplier;
        return label;
    }

    public Button CreateButton(Transform parent, string text, Action action)
    {
        var rect = CreateRect(text, parent, Vector2.zero, Vector2.one);
        rect.gameObject.AddComponent<Image>().color = ButtonColor;
        var button = rect.gameObject.AddComponent<Button>();
        button.onClick.AddListener(() =>
        {
            _playClick?.Invoke();
            action();
        });
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = ButtonHeight;
        CreateLabel(rect, text, ButtonFontSize);
        return button;
    }
}
