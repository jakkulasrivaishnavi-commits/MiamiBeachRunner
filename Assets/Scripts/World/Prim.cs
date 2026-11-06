using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Collider-free primitive meshes (cube / sphere / cylinder) used to assemble every 3D object.</summary>
public static class Prim
{
    static Mesh cube, sphere, cylinder, quad;

    static Mesh Grab(PrimitiveType type)
    {
        GameObject g = GameObject.CreatePrimitive(type);
        Mesh m = g.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(g);
        return m;
    }

    public static Mesh CubeMesh { get { if (cube == null) cube = Grab(PrimitiveType.Cube); return cube; } }
    public static Mesh SphereMesh { get { if (sphere == null) sphere = Grab(PrimitiveType.Sphere); return sphere; } }
    public static Mesh CylinderMesh { get { if (cylinder == null) cylinder = Grab(PrimitiveType.Cylinder); return cylinder; } }
    public static Mesh QuadMesh { get { if (quad == null) quad = Grab(PrimitiveType.Quad); return quad; } }

    public static GameObject Make(string name, Mesh mesh, Material mat, Transform parent, Vector3 pos, Vector3 scale, bool shadow)
    {
        GameObject go = new GameObject(name);
        Transform t = go.transform;
        t.SetParent(parent, false);
        t.localPosition = pos;
        t.localScale = scale;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = shadow ? ShadowCastingMode.On : ShadowCastingMode.Off;
        r.receiveShadows = true;
        return go;
    }

    public static GameObject Box(Transform parent, Vector3 pos, Vector3 size, Material mat, bool shadow = false)
    {
        return Make("Box", CubeMesh, mat, parent, pos, size, shadow);
    }

    public static GameObject Sphere(Transform parent, Vector3 pos, float diameter, Material mat, bool shadow = false)
    {
        return Make("Sphere", SphereMesh, mat, parent, pos, Vector3.one * diameter, shadow);
    }

    /// <summary>Cylinder with its axis along Y.</summary>
    public static GameObject Cyl(Transform parent, Vector3 pos, float diameter, float height, Material mat, bool shadow = false)
    {
        return Make("Cyl", CylinderMesh, mat, parent, pos, new Vector3(diameter, height * 0.5f, diameter), shadow);
    }

    public static GameObject Rot(GameObject go, Vector3 euler)
    {
        go.transform.localRotation = Quaternion.Euler(euler);
        return go;
    }

    public static Transform Pivot(string name, Transform parent, Vector3 pos)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        return g.transform;
    }
}
