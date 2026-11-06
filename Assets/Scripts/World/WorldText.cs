using UnityEngine;
using UnityEngine.UI;

/// <summary>Readable 3D text (neon signs, hotel names) built from a world-space uGUI canvas.</summary>
public static class WorldText
{
    /// <param name="euler">(0,0,0) faces the player; (0,-90,0) faces +X; (0,90,0) faces -X.</param>
    public static GameObject Create(Transform parent, string text, Color color, Vector3 localPos, Vector3 euler, float worldW, float worldH)
    {
        GameObject go = new GameObject("Text3D", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(worldW * 100f, worldH * 100f);
        rt.localScale = Vector3.one * 0.01f;
        rt.localPosition = localPos;
        rt.localRotation = Quaternion.Euler(euler);

        GameObject tgo = new GameObject("T", typeof(RectTransform));
        RectTransform trt = tgo.GetComponent<RectTransform>();
        trt.SetParent(go.transform, false);
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        Text t = tgo.AddComponent<Text>();
        t.font = UIKit.GetFont();
        t.text = text;
        t.color = color;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontSize = Mathf.RoundToInt(worldH * 100f * 0.8f);
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = 10;
        t.resizeTextMaxSize = Mathf.RoundToInt(worldH * 100f);
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.raycastTarget = false;
        return go;
    }
}
