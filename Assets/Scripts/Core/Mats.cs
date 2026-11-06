using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Creates every material and procedural texture the game uses at runtime, so the
/// project needs no art assets. Works in the Built-in pipeline and in URP.
/// </summary>
public static class Mats
{
    static Shader lit, sprite;
    static readonly Dictionary<string, Material> matCache = new Dictionary<string, Material>();
    static readonly Dictionary<string, Texture2D> texCache = new Dictionary<string, Texture2D>();

    public static Shader LitShader
    {
        get
        {
            if (lit != null) return lit;
            if (GraphicsSettings.currentRenderPipeline != null) lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) lit = Shader.Find("Standard");
            if (lit == null) lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) lit = Shader.Find("Sprites/Default");
            return lit;
        }
    }

    public static Shader SpriteShader
    {
        get
        {
            if (sprite == null) sprite = Shader.Find("Sprites/Default");
            if (sprite == null) sprite = LitShader;
            return sprite;
        }
    }

    static void SetF(Material m, string prop, float v)
    {
        if (m.HasProperty(prop)) m.SetFloat(prop, v);
    }

    /// <summary>Lit, coloured material.</summary>
    public static Material Solid(Color c, float smooth = 0.25f, float metal = 0f)
    {
        string k = "S" + ColorUtility.ToHtmlStringRGB(c) + smooth.ToString("F2") + metal.ToString("F2");
        Material m;
        if (matCache.TryGetValue(k, out m) && m != null) return m;
        m = new Material(LitShader);
        m.name = "MR_" + k;
        m.color = c;
        SetF(m, "_Smoothness", smooth);
        SetF(m, "_Glossiness", smooth);
        SetF(m, "_Metallic", metal);
        matCache[k] = m;
        return m;
    }

    /// <summary>Unlit, always-bright colour (neon tubes, sun, foam). Ignores lights and fog.</summary>
    public static Material Neon(Color c)
    {
        string k = "N" + ColorUtility.ToHtmlStringRGB(c);
        Material m;
        if (matCache.TryGetValue(k, out m) && m != null) return m;
        m = new Material(SpriteShader);
        m.name = "MR_" + k;
        m.color = c;
        matCache[k] = m;
        return m;
    }

    /// <summary>Lit material with a texture and tiling.</summary>
    public static Material Tex(Texture2D tex, Vector2 tiling, string key, float smooth = 0.2f)
    {
        string k = "T" + key;
        Material m;
        if (matCache.TryGetValue(k, out m) && m != null) return m;
        m = new Material(LitShader);
        m.name = "MR_" + k;
        m.color = Color.white;
        m.mainTexture = tex;
        m.mainTextureScale = tiling;
        SetF(m, "_Smoothness", smooth);
        SetF(m, "_Glossiness", smooth);
        matCache[k] = m;
        return m;
    }

    /// <summary>Unlit textured material (sky).</summary>
    public static Material Unlit(Texture2D tex, string key)
    {
        string k = "U" + key;
        Material m;
        if (matCache.TryGetValue(k, out m) && m != null) return m;
        m = new Material(SpriteShader);
        m.name = "MR_" + k;
        m.color = Color.white;
        m.mainTexture = tex;
        matCache[k] = m;
        return m;
    }

    // ------------------------------------------------------------------ textures

    static float Hash(int x, int y)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263;
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0xFFFF) / 65535f;
        }
    }

    static Color Shade(Color c, float k)
    {
        return new Color(Mathf.Clamp01(c.r * k), Mathf.Clamp01(c.g * k), Mathf.Clamp01(c.b * k), 1f);
    }

    static Texture2D Make(string key, int w, int h, System.Func<int, int, Color> f, TextureWrapMode wrap, bool mips)
    {
        Texture2D t;
        if (texCache.TryGetValue(key, out t) && t != null) return t;
        t = new Texture2D(w, h, TextureFormat.RGBA32, mips);
        t.name = "MR_" + key;
        Color[] px = new Color[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                px[y * w + x] = f(x, y);
        t.SetPixels(px);
        t.wrapMode = wrap;
        t.filterMode = FilterMode.Bilinear;
        t.anisoLevel = 4;
        t.Apply(mips);
        texCache[key] = t;
        return t;
    }

    /// <summary>Boardwalk planks (one plank per tile along V).</summary>
    public static Texture2D WoodTex()
    {
        Color baseC = GameConfig.RGB(176, 124, 78);
        return Make("wood", 16, 32, (x, y) =>
        {
            float grain = 0.93f + 0.07f * Mathf.Sin(x * 2.3f + y * 0.15f);
            Color c = Shade(baseC, grain);
            if (y < 2 || y > 29) c = Shade(baseC, 0.55f);   // gap between planks
            return c;
        }, TextureWrapMode.Repeat, true);
    }

    public static Texture2D SandTex()
    {
        Color baseC = GameConfig.RGB(244, 218, 165);
        return Make("sand", 64, 64, (x, y) =>
        {
            float n = Hash(x, y);
            float k = 0.93f + 0.10f * n;
            if (n > 0.985f) k = 0.78f;
            return Shade(baseC, k);
        }, TextureWrapMode.Repeat, true);
    }

    /// <summary>Art-deco wall with a sunset-reflecting window. One tile = one window.</summary>
    public static Texture2D WallTex(int idx)
    {
        Color wall = GameConfig.Pastels[idx % GameConfig.Pastels.Length];
        return Make("wall" + idx, 64, 64, (x, y) =>
        {
            Color c = Shade(wall, 0.90f + 0.10f * (y / 63f));
            bool inWin = x >= 14 && x < 50 && y >= 12 && y < 50;
            bool frame = x >= 11 && x < 53 && y >= 9 && y < 53 && !inWin;
            if (inWin)
            {
                float k = Mathf.Clamp01((y - 12f) / 38f);
                c = Color.Lerp(GameConfig.RGB(40, 110, 150), GameConfig.RGB(255, 150, 130), Mathf.Clamp01((k - 0.35f) * 1.7f));
                if (x >= 31 && x <= 32) c = Color.Lerp(c, Color.white, 0.35f);
            }
            else if (frame) c = GameConfig.RGB(252, 250, 246);
            else if (y >= 58) c = Shade(wall, 0.78f);            // deco eyebrow line
            return c;
        }, TextureWrapMode.Repeat, true);
    }

    /// <summary>Synthwave sunset sky with a striped sun and a Miami skyline silhouette.</summary>
    public static Texture2D SkyTex()
    {
        Texture2D cached;
        if (texCache.TryGetValue("sky", out cached) && cached != null) return cached;

        int W = 512, H = 256;
        Color[] px = new Color[W * H];
        float horizon = 0.30f;
        int hy = (int)(horizon * H);
        Color orange = GameConfig.RGB(255, 168, 92);
        Color pink = GameConfig.RGB(255, 98, 140);
        Color mag = GameConfig.RGB(184, 40, 134);
        Color top = GameConfig.RGB(28, 10, 76);

        for (int y = 0; y < H; y++)
        {
            float v = y / (float)(H - 1);
            Color c;
            if (v <= horizon) c = orange;
            else
            {
                float k = (v - horizon) / (1f - horizon);
                if (k < 0.22f) c = Color.Lerp(orange, pink, k / 0.22f);
                else if (k < 0.55f) c = Color.Lerp(pink, mag, (k - 0.22f) / 0.33f);
                else c = Color.Lerp(mag, top, Mathf.Clamp01((k - 0.55f) / 0.45f));
            }
            for (int x = 0; x < W; x++) px[y * W + x] = c;
        }

        System.Random rnd = new System.Random(5);
        for (int i = 0; i < 110; i++)
        {
            int sx = rnd.Next(W);
            int sy = rnd.Next((int)(H * 0.62f), H);
            float a = 0.35f + (float)rnd.NextDouble() * 0.65f;
            px[sy * W + sx] = Color.Lerp(px[sy * W + sx], Color.white, a);
        }

        // sun
        float cx = W * 0.5f, cy = H * 0.43f, r = 40f, R = r * 2.8f;
        Color glow = GameConfig.RGB(255, 140, 100);
        for (int y = Mathf.Max(hy, (int)(cy - R)); y < Mathf.Min(H, (int)(cy + R)); y++)
        {
            for (int x = Mathf.Max(0, (int)(cx - R)); x < Mathf.Min(W, (int)(cx + R)); x++)
            {
                float dx = x - cx, dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d < R)
                {
                    float g = Mathf.Pow(1f - d / R, 2.2f) * 0.55f;
                    px[y * W + x] = Color.Lerp(px[y * W + x], glow, g);
                }
                if (d < r + 1f)
                {
                    float t = Mathf.Clamp01((y - (cy - r)) / (2f * r));
                    Color sc = Color.Lerp(GameConfig.RGB(255, 70, 130), GameConfig.RGB(255, 236, 110), t);
                    if (t < 0.55f)
                    {
                        float k = (0.55f - t) / 0.55f;
                        float thick = k * 5.5f;
                        float ph = (y - (cy - r)) % 9f;
                        if (ph < thick) continue;
                    }
                    float a = Mathf.Clamp01(r - d + 0.5f);
                    px[y * W + x] = Color.Lerp(px[y * W + x], sc, a);
                }
            }
        }

        // skyline silhouette
        Color sil = GameConfig.RGB(52, 16, 84);
        int bx = 0;
        while (bx < W)
        {
            int bw = rnd.Next(5, 14);
            int bh = rnd.Next(6, 30);
            for (int x = bx; x < Mathf.Min(W, bx + bw); x++)
                for (int y = hy; y < Mathf.Min(H, hy + bh); y++)
                {
                    Color c = sil;
                    if (y > hy + 2 && ((x - bx) % 3 == 1) && (y % 4 == 1) && rnd.NextDouble() < 0.35) c = GameConfig.RGB(255, 205, 110);
                    px[y * W + x] = c;
                }
            bx += bw + rnd.Next(0, 3);
        }

        Texture2D t2 = new Texture2D(W, H, TextureFormat.RGBA32, false);
        t2.name = "MR_sky";
        t2.SetPixels(px);
        t2.wrapMode = TextureWrapMode.Clamp;
        t2.filterMode = FilterMode.Bilinear;
        t2.Apply(false);
        texCache["sky"] = t2;
        return t2;
    }
}
