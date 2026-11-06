using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Tiny flying cubes used for coin sparkles and the crash explosion.</summary>
public class Debris : MonoBehaviour
{
    Vector3 vel, spin, baseScale;
    float life, age;

    public static void Burst(Vector3 pos, Color color, int count, float speed, float life, float size = 0.14f)
    {
        Material m = Mats.Neon(color);
        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject("Debris");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * size * Random.Range(0.6f, 1.4f);
            go.AddComponent<MeshFilter>().sharedMesh = Prim.CubeMesh;
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            Debris d = go.AddComponent<Debris>();
            Vector3 v = Random.onUnitSphere * speed * Random.Range(0.4f, 1f);
            v.y = Mathf.Abs(v.y) + speed * 0.3f;
            d.vel = v;
            d.spin = Random.insideUnitSphere * 360f;
            d.life = life;
            d.baseScale = go.transform.localScale;
        }
    }

    void Update()
    {
        age += Time.deltaTime;
        vel += Vector3.down * 14f * Time.deltaTime;
        transform.position += vel * Time.deltaTime;
        transform.Rotate(spin * Time.deltaTime);
        transform.localScale = baseScale * Mathf.Max(0f, 1f - age / life);
        if (age >= life) Destroy(gameObject);
    }
}
