using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Camera, sunset lighting, fog and sky for every 3D scene.</summary>
public static class WorldSetup
{
    public static readonly Color Horizon = new Color(1f, 0.659f, 0.361f);

    public static Camera GetCamera()
    {
        Camera c = Camera.main;
        if (c == null) c = UnityUtil.FindFirst<Camera>();
        if (c == null)
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            c = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }
        return c;
    }

    public static Camera Apply(bool withSky = true)
    {
        Camera cam = GetCamera();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Horizon;
        cam.nearClipPlane = 0.2f;
        cam.farClipPlane = 900f;
        cam.fieldOfView = 62f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = Horizon;
        RenderSettings.fogStartDistance = 80f;
        RenderSettings.fogEndDistance = 225f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.62f, 0.50f, 0.66f);
        QualitySettings.shadowDistance = 70f;

        // key light (warm sunset, from behind the camera so the player is lit)
        Light key = null;
        foreach (Light l in UnityUtil.FindAll<Light>())
            if (l.type == LightType.Directional && l.gameObject.name != "RimLight") { key = l; break; }
        if (key == null)
        {
            GameObject go = new GameObject("Sun");
            key = go.AddComponent<Light>();
            key.type = LightType.Directional;
        }
        key.color = new Color(1f, 0.80f, 0.62f);
        key.intensity = 1.25f;
        key.transform.rotation = Quaternion.Euler(42f, 25f, 0f);
        key.shadows = LightShadows.Soft;
        key.shadowStrength = 0.65f;
        RenderSettings.sun = key;

        // magenta rim light from the sun's direction
        if (GameObject.Find("RimLight") == null)
        {
            GameObject rim = new GameObject("RimLight");
            Light rl = rim.AddComponent<Light>();
            rl.type = LightType.Directional;
            rl.color = new Color(1f, 0.35f, 0.65f);
            rl.intensity = 0.45f;
            rl.shadows = LightShadows.None;
            rim.transform.rotation = Quaternion.Euler(12f, 200f, 0f);
        }

        if (withSky && UnityUtil.FindFirst<SkyDome>() == null)
        {
            GameObject sky = Prim.Make("SkyDome", Prim.QuadMesh, Mats.Unlit(Mats.SkyTex(), "sky"), null, Vector3.zero, new Vector3(1000f, 500f, 1f), false);
            SkyDome sd = sky.AddComponent<SkyDome>();
            sd.cam = cam.transform;
        }
        return cam;
    }
}
