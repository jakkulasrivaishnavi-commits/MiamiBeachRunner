using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

/// <summary>Builds uGUI elements from code so scenes stay empty and the project needs no prefabs.</summary>
public static class UIKit
{
    static Font font;
    static Sprite rounded, circle;

    public static Font GetFont()
    {
        if (font != null) return font;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch (System.Exception) { }
        if (font == null)
        {
            try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch (System.Exception) { }
        }
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 32);
        return font;
    }

    public static Sprite Rounded
    {
        get
        {
            if (rounded != null) return rounded;
            const int S = 64; const float rad = 20f;
            Texture2D t = new Texture2D(S, S, TextureFormat.RGBA32, false);
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = Mathf.Max(Mathf.Abs(x - 31.5f) - (32f - rad), 0f);
                    float dy = Mathf.Max(Mathf.Abs(y - 31.5f) - (32f - rad), 0f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    t.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(rad - d + 0.5f)));
                }
            t.wrapMode = TextureWrapMode.Clamp; t.filterMode = FilterMode.Bilinear; t.Apply();
            rounded = Sprite.Create(t, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(24, 24, 24, 24));
            return rounded;
        }
    }

    public static Sprite Circle
    {
        get
        {
            if (circle != null) return circle;
            const int S = 64;
            Texture2D t = new Texture2D(S, S, TextureFormat.RGBA32, false);
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float d = Mathf.Sqrt((x - 31.5f) * (x - 31.5f) + (y - 31.5f) * (y - 31.5f));
                    t.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(31.5f - d + 0.5f)));
                }
            t.wrapMode = TextureWrapMode.Clamp; t.filterMode = FilterMode.Bilinear; t.Apply();
            circle = Sprite.Create(t, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 100f);
            return circle;
        }
    }

    public static Sprite GradientSprite(Color top, Color bottom)
    {
        Texture2D t = new Texture2D(2, 128, TextureFormat.RGBA32, false);
        for (int y = 0; y < 128; y++)
            for (int x = 0; x < 2; x++)
                t.SetPixel(x, y, Color.Lerp(bottom, top, y / 127f));
        t.wrapMode = TextureWrapMode.Clamp; t.filterMode = FilterMode.Bilinear; t.Apply();
        return Sprite.Create(t, new Rect(0, 0, 2, 128), new Vector2(0.5f, 0.5f), 100f);
    }

    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = sortingOrder;
        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920f, 1080f);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    public static void EnsureEventSystem()
    {
        if (UnityUtil.FindFirst<EventSystem>() != null) return;
        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        InputSystemUIInputModule m = go.AddComponent<InputSystemUIInputModule>();
        m.AssignDefaultActions();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    public static RectTransform MakeRect(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = pivot;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return rt;
    }

    /// <summary>Full-screen coloured image.</summary>
    public static Image Full(Transform parent, Color color)
    {
        GameObject go = new GameObject("Full", typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    /// <summary>Rounded panel, centre-pivoted at the given anchor.</summary>
    public static Image Box(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color, bool round = true)
    {
        RectTransform rt = MakeRect(parent, name, anchor, new Vector2(0.5f, 0.5f), pos, size);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        if (round) { img.sprite = Rounded; img.type = Image.Type.Sliced; }
        img.raycastTarget = false;
        return img;
    }

    public static Image Icon(Transform parent, Vector2 anchor, Vector2 pos, float size, Color color, string glyph, int glyphSize)
    {
        RectTransform rt = MakeRect(parent, "Icon", anchor, new Vector2(0.5f, 0.5f), pos, new Vector2(size, size));
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = Circle; img.color = color; img.raycastTarget = false;
        Label(rt, glyph, glyphSize, new Color(0.25f, 0.12f, 0f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size), TextAnchor.MiddleCenter, true, false);
        return img;
    }

    public static Text Label(Transform parent, string text, int size, Color color, Vector2 anchor, Vector2 pos, Vector2 sz,
        TextAnchor align = TextAnchor.MiddleCenter, bool bold = true, bool outline = true)
    {
        RectTransform rt = MakeRect(parent, "Label", anchor, new Vector2(0.5f, 0.5f), pos, sz);
        Text t = rt.gameObject.AddComponent<Text>();
        t.font = GetFont();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        if (outline)
        {
            Outline o = rt.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.55f);
            o.effectDistance = new Vector2(2f, -2f);
        }
        return t;
    }

    public static Button MakeButton(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size,
        Color bg, Color fg, int fontSize, UnityAction onClick)
    {
        RectTransform rt = MakeRect(parent, "Btn_" + label, anchor, new Vector2(0.5f, 0.5f), pos, size);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = Rounded; img.type = Image.Type.Sliced; img.color = bg;
        Button b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        ColorBlock cb = b.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1f, 0.92f, 0.97f);
        cb.pressedColor = new Color(0.75f, 0.75f, 0.8f);
        cb.selectedColor = Color.white;
        cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.8f);
        cb.fadeDuration = 0.08f;
        b.colors = cb;
        Label(rt, label, fontSize, fg, new Vector2(0.5f, 0.5f), Vector2.zero, size, TextAnchor.MiddleCenter, true, false);
        b.onClick.AddListener(() => { AudioManager.Play(Sfx.Click); onClick(); });
        return b;
    }

    public static Slider MakeSlider(Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, float value, UnityAction<float> onChange)
    {
        RectTransform root = MakeRect(parent, "Slider", anchor, new Vector2(0.5f, 0.5f), pos, size);
        Slider slider = root.gameObject.AddComponent<Slider>();

        RectTransform bg = MakeRect(root, "Background", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size.x, size.y * 0.35f));
        Image bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.sprite = Rounded; bgImg.type = Image.Type.Sliced; bgImg.color = new Color(1f, 1f, 1f, 0.22f);

        GameObject fa = new GameObject("Fill Area", typeof(RectTransform));
        RectTransform faRt = fa.GetComponent<RectTransform>();
        faRt.SetParent(root, false);
        faRt.anchorMin = new Vector2(0f, 0.5f); faRt.anchorMax = new Vector2(1f, 0.5f);
        faRt.sizeDelta = new Vector2(-size.y, size.y * 0.35f); faRt.anchoredPosition = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.SetParent(faRt, false);
        fillRt.anchorMin = new Vector2(0f, 0f); fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.sprite = Rounded; fillImg.type = Image.Type.Sliced; fillImg.color = GameConfig.Cyan;

        GameObject ha = new GameObject("Handle Slide Area", typeof(RectTransform));
        RectTransform haRt = ha.GetComponent<RectTransform>();
        haRt.SetParent(root, false);
        haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(size.y * 0.5f, 0f); haRt.offsetMax = new Vector2(-size.y * 0.5f, 0f);

        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        RectTransform hRt = handle.GetComponent<RectTransform>();
        hRt.SetParent(haRt, false);
        hRt.anchorMin = new Vector2(0f, 0f); hRt.anchorMax = new Vector2(0f, 1f);
        hRt.pivot = new Vector2(0.5f, 0.5f);
        hRt.sizeDelta = new Vector2(size.y * 0.8f, 0f);
        Image hImg = handle.AddComponent<Image>();
        hImg.sprite = Circle; hImg.color = Color.white;

        slider.fillRect = fillRt;
        slider.handleRect = hRt;
        slider.targetGraphic = hImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f; slider.maxValue = 1f;
        slider.value = value;
        slider.onValueChanged.AddListener(onChange);
        return slider;
    }

    /// <summary>Master / Music / SFX sliders (defaults 100 / 70 / 80 as in the PPT).</summary>
    public static void AddVolumeSliders(Transform parent, float topY)
    {
        string[] names = { "MASTER", "MUSIC", "SFX" };
        for (int i = 0; i < 3; i++)
        {
            int which = i;
            float y = topY - i * 85f;
            Label(parent, names[i], 34, Color.white, new Vector2(0.5f, 0.5f), new Vector2(-300f, y), new Vector2(220f, 60f), TextAnchor.MiddleLeft, true, false);
            Text val = Label(parent, Mathf.RoundToInt(AudioManager.GetVolume(which) * 100f) + "%", 32, GameConfig.Sun,
                new Vector2(0.5f, 0.5f), new Vector2(330f, y), new Vector2(120f, 60f), TextAnchor.MiddleRight, true, false);
            MakeSlider(parent, new Vector2(0.5f, 0.5f), new Vector2(30f, y), new Vector2(420f, 52f), AudioManager.GetVolume(which), v =>
            {
                AudioManager.SetVolume(which, v);
                val.text = Mathf.RoundToInt(v * 100f) + "%";
            });
        }
    }
}
