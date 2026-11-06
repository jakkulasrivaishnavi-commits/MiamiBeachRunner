using UnityEngine;

/// <summary>
/// Builds every Miami Beach prop procedurally: boardwalk, sand, ocean, palms, art-deco hotels,
/// lifeguard towers, umbrellas, sailboats, neon arch - plus the obstacles and collectibles.
/// All positions are local to the track segment (x = across the track, z = along it).
/// </summary>
public static class WorldBuilder
{
    static readonly string[] HotelNames =
    {
        "HOTEL AZURE", "THE FLAMINGO", "CARDOZO", "PALM BAR", "OCEAN DR", "SOUTH BEACH",
        "LA SIRENA", "CAFE SOL", "SURF CLUB", "THE COLONY", "MIAMI VICE", "TIKI LOUNGE"
    };
    static readonly string[] SignNames =
    {
        "OCEAN DR", "SOUTH BEACH", "SURF SHOP", "MIAMI", "SUNSET BLVD", "TIKI BAR", "ART DECO", "LINCOLN RD"
    };
    static readonly float[] BuildingHeights = { 9f, 12f, 15f, 18f };

    static Color C(int r, int g, int b) { return GameConfig.RGB(r, g, b); }

    static Transform NewRoot(string name, Transform parent, Vector3 pos)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        return g.transform;
    }

    // ================================================================== GROUND

    public static void Ground(Transform seg)
    {
        float L = GameConfig.SegmentLength;
        float cz = L * 0.5f;

        Material wood = Mats.Tex(Mats.WoodTex(), new Vector2(3f, L), "wood", 0.15f);
        Prim.Box(seg, new Vector3(0f, -0.25f, cz), new Vector3(9f, 0.5f, L), wood);

        Material curb = Mats.Solid(C(250, 246, 240), 0.3f);
        Prim.Box(seg, new Vector3(-4.65f, 0.0f, cz), new Vector3(0.3f, 0.3f, L), curb);
        Prim.Box(seg, new Vector3(4.65f, 0.0f, cz), new Vector3(0.3f, 0.3f, L), curb);
        Prim.Box(seg, new Vector3(-4.65f, 0.16f, cz), new Vector3(0.1f, 0.02f, L), Mats.Neon(GameConfig.Cyan));
        Prim.Box(seg, new Vector3(4.65f, 0.16f, cz), new Vector3(0.1f, 0.02f, L), Mats.Neon(GameConfig.HotPink));

        // lane lines
        Prim.Box(seg, new Vector3(-1.25f, 0.012f, cz), new Vector3(0.07f, 0.02f, L), Mats.Neon(GameConfig.HotPink));
        Prim.Box(seg, new Vector3(1.25f, 0.012f, cz), new Vector3(0.07f, 0.02f, L), Mats.Neon(GameConfig.Cyan));

        // sand (left and right) + wet sand + foam + water
        Material sandL = Mats.Tex(Mats.SandTex(), new Vector2(8f, L / 4f), "sandL", 0.05f);
        Material sandR = Mats.Tex(Mats.SandTex(), new Vector2(4f, L / 4f), "sandR", 0.05f);
        Prim.Box(seg, new Vector3(-17.5f, -0.35f, cz), new Vector3(26f, 0.5f, L), sandL);
        Prim.Box(seg, new Vector3(10.75f, -0.35f, cz), new Vector3(12.5f, 0.5f, L), sandR);
        Prim.Box(seg, new Vector3(16.5f, -0.34f, cz), new Vector3(3f, 0.5f, L), Mats.Solid(C(205, 170, 120), 0.5f));
        Prim.Box(seg, new Vector3(17.6f, -0.30f, cz), new Vector3(0.9f, 0.4f, L), Mats.Neon(C(250, 250, 252)));
        Prim.Box(seg, new Vector3(26f, -0.55f, cz), new Vector3(17f, 0.4f, L), Mats.Solid(C(60, 225, 215), 0.9f));
        Prim.Box(seg, new Vector3(120f, -0.6f, cz), new Vector3(171f, 0.4f, L), Mats.Solid(C(10, 140, 190), 0.9f));

        // far-left city ground
        Prim.Box(seg, new Vector3(-95f, -0.4f, cz), new Vector3(129f, 0.5f, L), Mats.Solid(C(60, 36, 96), 0.1f));
    }

    // ================================================================== SCENERY

    public static void Scenery(Transform parent, int index)
    {
        float L = GameConfig.SegmentLength;

        // art-deco hotels along Ocean Drive (left)
        float[] zc = { 9f, 30f, 51f };
        for (int i = 0; i < 3; i++)
        {
            float h = BuildingHeights[Random.Range(0, BuildingHeights.Length)];
            Building(parent, new Vector3(-16f, 0f, zc[i]), 17f, h, Random.Range(0, GameConfig.Pastels.Length),
                HotelNames[(index * 3 + i) % HotelNames.Length], i);
        }

        // distant skyline slabs
        for (int i = 0; i < 2; i++)
        {
            float h = Random.Range(26f, 48f);
            float z = 15f + i * 30f + Random.Range(-4f, 4f);
            Color c = Color.Lerp(C(86, 50, 128), C(130, 70, 150), Random.value);
            Prim.Box(parent, new Vector3(-40f - Random.Range(0f, 8f), h * 0.5f, z), new Vector3(14f, h, 16f), Mats.Solid(c, 0.1f));
            Prim.Box(parent, new Vector3(-32.9f, h - 0.5f, z), new Vector3(0.2f, 0.2f, 16f), Mats.Neon(GameConfig.NeonColors[Random.Range(0, GameConfig.NeonColors.Length)]));
        }

        // palm trees both sides of the boardwalk
        float[] pzL = { 7f, 22f, 37f, 52f };
        float[] pzR = { 14f, 29f, 44f, 59f };
        for (int i = 0; i < 4; i++)
        {
            Palm(parent, new Vector3(-6.0f + Random.Range(-0.4f, 0.4f), -0.1f, pzL[i]), Random.Range(5f, 7f), Random.Range(10f, 25f), 90f + Random.Range(-30f, 30f));
            Palm(parent, new Vector3(6.2f + Random.Range(-0.4f, 0.4f), -0.1f, pzR[i]), Random.Range(5f, 7f), Random.Range(10f, 25f), -90f + Random.Range(-30f, 30f));
        }

        // neon street lamps on the boardwalk edge
        Lamp(parent, new Vector3(-4.95f, 0f, 0.5f), GameConfig.Cyan);
        Lamp(parent, new Vector3(4.95f, 0f, 15.5f), GameConfig.HotPink);
        Lamp(parent, new Vector3(-4.95f, 0f, 30.5f), GameConfig.Cyan);
        Lamp(parent, new Vector3(4.95f, 0f, 45.5f), GameConfig.HotPink);

        // beach props (right)
        float[] bz = { 10f, 30f, 50f };
        for (int i = 0; i < 3; i++) BeachSlot(parent, 8.5f + Random.Range(0f, 4.5f), bz[i] + Random.Range(-3f, 3f));

        // sailboats
        if (Random.value < 0.7f) Boat(parent, new Vector3(Random.Range(40f, 85f), -0.25f, Random.Range(5f, L - 5f)));

        // Miami Beach neon arch
        if (index % 8 == 3) Arch(parent, 30f);
    }

    public static void Building(Transform parent, Vector3 pos, float depth, float h, int colorIdx, string name, int slot)
    {
        Transform root = NewRoot("Hotel", parent, pos);
        Material wall = Mats.Tex(Mats.WallTex(colorIdx), new Vector2(depth / 4f, h / 3.5f), "wall" + colorIdx + "_" + h, 0.35f);
        Prim.Box(root, new Vector3(0f, h * 0.5f - 0.3f, 0f), new Vector3(10f, h + 0.6f, depth), wall);

        Color accent = GameConfig.Pastels[(colorIdx + 3) % GameConfig.Pastels.Length];
        if (h >= 15f)
            Prim.Box(root, new Vector3(0f, h + 1.5f, 0f), new Vector3(7f, 3f, depth - 4f), Mats.Solid(accent, 0.3f));

        Color neonA = GameConfig.NeonColors[(colorIdx + slot) % GameConfig.NeonColors.Length];
        Color neonB = GameConfig.NeonColors[(colorIdx + slot + 2) % GameConfig.NeonColors.Length];
        Prim.Box(root, new Vector3(5.05f, h + 0.05f, 0f), new Vector3(0.2f, 0.25f, depth), Mats.Neon(neonA));
        Prim.Box(root, new Vector3(5.05f, 4.2f, 0f), new Vector3(0.15f, 0.15f, depth), Mats.Neon(neonB));

        // awning
        Prim.Box(root, new Vector3(5.8f, 3.1f, 0f), new Vector3(1.8f, 0.15f, depth * 0.75f), Mats.Solid(GameConfig.NeonColors[(slot + colorIdx + 1) % 5] * 0.85f, 0.3f));

        // hotel sign facing the boardwalk (+X)
        Prim.Box(root, new Vector3(5.04f, Mathf.Min(h * 0.62f, 11f), 0f), new Vector3(0.1f, 1.7f, 9f), Mats.Solid(GameConfig.Navy, 0.6f));
        WorldText.Create(root, name, neonA, new Vector3(5.12f, Mathf.Min(h * 0.62f, 11f), 0f), new Vector3(0f, -90f, 0f), 8.4f, 1.4f);
    }

    public static void Palm(Transform parent, Vector3 pos, float height, float lean, float yaw)
    {
        Transform root = NewRoot("Palm", parent, pos);
        root.localRotation = Quaternion.Euler(0f, yaw, 0f);
        Material trunk = Mats.Solid(C(140, 96, 58), 0.1f);
        Material ring = Mats.Solid(C(110, 74, 44), 0.1f);
        Material leafA = Mats.Solid(C(24, 150, 70), 0.3f);
        Material leafB = Mats.Solid(C(60, 190, 85), 0.3f);
        Material nut = Mats.Solid(C(90, 56, 30), 0.3f);

        int segs = 4;
        float segH = height / segs;
        Transform cur = root;
        for (int i = 0; i < segs; i++)
        {
            Transform pv = Prim.Pivot("t" + i, cur, i == 0 ? Vector3.zero : new Vector3(0f, segH, 0f));
            pv.localRotation = Quaternion.Euler(lean / segs, 0f, 0f);
            float d = Mathf.Lerp(0.5f, 0.3f, i / (float)segs);
            Prim.Cyl(pv, new Vector3(0f, segH * 0.5f, 0f), d, segH * 1.02f, (i % 2 == 0) ? trunk : ring, true);
            cur = pv;
        }
        Transform top = Prim.Pivot("Top", cur, new Vector3(0f, segH, 0f));
        for (int k = 0; k < 7; k++)
        {
            Transform fp = Prim.Pivot("Frond", top, Vector3.zero);
            fp.localRotation = Quaternion.Euler(22f + (k % 2) * 14f, k * (360f / 7f) + Random.Range(-8f, 8f), 0f);
            Prim.Box(fp, new Vector3(0f, 0f, 1.25f), new Vector3(0.55f, 0.05f, 2.5f), (k % 2 == 0) ? leafA : leafB, true);
        }
        Prim.Sphere(top, new Vector3(0.15f, -0.2f, 0.1f), 0.28f, nut);
        Prim.Sphere(top, new Vector3(-0.15f, -0.25f, -0.1f), 0.26f, nut);
    }

    static void Lamp(Transform parent, Vector3 pos, Color neon)
    {
        Transform root = NewRoot("Lamp", parent, pos);
        Prim.Cyl(root, new Vector3(0f, 1.6f, 0f), 0.12f, 3.2f, Mats.Solid(C(40, 30, 70), 0.5f));
        Prim.Sphere(root, new Vector3(0f, 3.3f, 0f), 0.42f, Mats.Neon(neon));
        Prim.Sphere(root, new Vector3(0f, 3.3f, 0f), 0.7f, Mats.Neon(new Color(neon.r, neon.g, neon.b, 0.25f)));
    }

    static void BeachSlot(Transform parent, float x, float z)
    {
        float r = Random.value;
        Vector3 p = new Vector3(x, -0.1f, z);
        if (r < 0.28f) Lifeguard(parent, p, GameConfig.Pastels[Random.Range(0, GameConfig.Pastels.Length)]);
        else if (r < 0.55f)
        {
            Umbrella(parent, p, GameConfig.NeonColors[Random.Range(0, 5)]);
            Umbrella(parent, p + new Vector3(2.6f, 0f, 1.2f), GameConfig.NeonColors[Random.Range(0, 5)]);
        }
        else if (r < 0.78f)
        {
            Surfboard(parent, p, GameConfig.NeonColors[Random.Range(0, 5)]);
            Surfboard(parent, p + new Vector3(0.7f, 0f, 0.3f), GameConfig.NeonColors[Random.Range(0, 5)]);
            Surfboard(parent, p + new Vector3(1.4f, 0f, -0.2f), GameConfig.NeonColors[Random.Range(0, 5)]);
        }
        else VolleyNet(parent, p);
    }

    static void Umbrella(Transform parent, Vector3 pos, Color color)
    {
        Transform root = NewRoot("Umbrella", parent, pos);
        Prim.Cyl(root, new Vector3(0f, 1.1f, 0f), 0.07f, 2.2f, Mats.Solid(C(240, 240, 240), 0.5f));
        GameObject canopy = Prim.Sphere(root, new Vector3(0f, 2.25f, 0f), 1f, Mats.Solid(color, 0.3f), true);
        canopy.transform.localScale = new Vector3(2.5f, 0.6f, 2.5f);
        Prim.Box(root, new Vector3(0.9f, 0.03f, 0.3f), new Vector3(1.0f, 0.04f, 2.0f), Mats.Solid(Color.Lerp(color, Color.white, 0.6f), 0.2f));
    }

    static void Lifeguard(Transform parent, Vector3 pos, Color color)
    {
        Transform root = NewRoot("LifeguardTower", parent, pos);
        Material wood = Mats.Solid(C(250, 245, 238), 0.3f);
        Material body = Mats.Solid(color, 0.3f);
        for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
                Prim.Box(root, new Vector3(sx * 0.6f, 0.75f, sz * 0.6f), new Vector3(0.12f, 1.5f, 0.12f), wood, true);
        Prim.Box(root, new Vector3(0f, 1.5f, 0f), new Vector3(1.7f, 0.12f, 1.7f), wood, true);
        Prim.Box(root, new Vector3(0f, 2.2f, 0f), new Vector3(1.4f, 1.3f, 1.4f), body, true);
        Prim.Box(root, new Vector3(-0.72f, 2.3f, 0f), new Vector3(0.05f, 0.5f, 0.7f), Mats.Solid(C(25, 35, 70), 0.9f));
        Prim.Box(root, new Vector3(0f, 2.95f, 0f), new Vector3(2.0f, 0.14f, 2.0f), Mats.Solid(GameConfig.HotPink, 0.3f), true);
        Prim.Box(root, new Vector3(-0.72f, 1.8f, 0f), new Vector3(0.06f, 0.1f, 1.4f), Mats.Neon(GameConfig.Cyan));
        Prim.Cyl(root, new Vector3(0.5f, 3.6f, 0.5f), 0.05f, 1.2f, wood);
        GameObject flag = Prim.Box(root, new Vector3(0.5f, 4.05f, 0.9f), new Vector3(0.03f, 0.35f, 0.7f), Mats.Neon(GameConfig.Orange));
        flag.AddComponent<Bob>();
        Prim.Box(root, new Vector3(-1.1f, 0.8f, 0f), new Vector3(0.08f, 1.8f, 0.5f), wood);
    }

    static void Surfboard(Transform parent, Vector3 pos, Color color)
    {
        Transform root = NewRoot("Surfboard", parent, pos);
        root.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-8f, 8f));
        Prim.Box(root, new Vector3(0f, 0.9f, 0f), new Vector3(0.5f, 1.9f, 0.07f), Mats.Solid(color, 0.6f), true);
        Prim.Box(root, new Vector3(0f, 1.0f, 0f), new Vector3(0.52f, 0.12f, 0.08f), Mats.Solid(Color.white, 0.4f));
    }

    static void VolleyNet(Transform parent, Vector3 pos)
    {
        Transform root = NewRoot("VolleyNet", parent, pos);
        Material pole = Mats.Solid(C(230, 230, 235), 0.4f);
        Prim.Cyl(root, new Vector3(0f, 1.2f, -2.2f), 0.09f, 2.4f, pole);
        Prim.Cyl(root, new Vector3(0f, 1.2f, 2.2f), 0.09f, 2.4f, pole);
        Prim.Box(root, new Vector3(0f, 2.0f, 0f), new Vector3(0.03f, 1.0f, 4.4f), Mats.Solid(C(235, 235, 240), 0.1f));
        Prim.Box(root, new Vector3(0f, 2.5f, 0f), new Vector3(0.05f, 0.08f, 4.4f), Mats.Neon(GameConfig.Lime));
        Prim.Sphere(root, new Vector3(1.5f, 0.2f, 1.0f), 0.4f, Mats.Solid(C(255, 255, 255), 0.5f));
    }

    static void Boat(Transform parent, Vector3 pos)
    {
        Transform root = NewRoot("Sailboat", parent, pos);
        Transform bob = Prim.Pivot("Bob", root, Vector3.zero);
        Bob b = bob.gameObject.AddComponent<Bob>();
        b.amplitude = 0.15f; b.speed = 1.0f; b.roll = 4f;
        Prim.Box(bob, new Vector3(0f, 0.25f, 0f), new Vector3(1.0f, 0.6f, 3.2f), Mats.Solid(C(250, 250, 250), 0.5f));
        Prim.Box(bob, new Vector3(0f, 0.58f, 0f), new Vector3(1.04f, 0.1f, 3.25f), Mats.Solid(GameConfig.HotPink, 0.4f));
        Prim.Cyl(bob, new Vector3(0f, 2.2f, 0f), 0.08f, 4f, Mats.Solid(C(200, 200, 205), 0.5f));
        Prim.Box(bob, new Vector3(0f, 2.4f, -0.5f), new Vector3(0.04f, 3.0f, 1.8f), Mats.Neon(C(255, 245, 235)));
    }

    static void Arch(Transform parent, float z)
    {
        Transform root = NewRoot("MiamiArch", parent, new Vector3(0f, 0f, z));
        Material post = Mats.Solid(C(250, 245, 238), 0.3f);
        Prim.Box(root, new Vector3(-5.5f, 4.5f, 0f), new Vector3(0.7f, 9f, 0.7f), post, true);
        Prim.Box(root, new Vector3(5.5f, 4.5f, 0f), new Vector3(0.7f, 9f, 0.7f), post, true);
        Prim.Box(root, new Vector3(0f, 9.2f, 0f), new Vector3(12.2f, 2.0f, 0.6f), Mats.Solid(GameConfig.Navy, 0.5f), true);
        Prim.Box(root, new Vector3(0f, 10.25f, -0.3f), new Vector3(12.2f, 0.12f, 0.12f), Mats.Neon(GameConfig.HotPink));
        Prim.Box(root, new Vector3(0f, 8.15f, -0.3f), new Vector3(12.2f, 0.12f, 0.12f), Mats.Neon(GameConfig.Cyan));
        WorldText.Create(root, "MIAMI BEACH", GameConfig.Sun, new Vector3(0f, 9.2f, -0.4f), Vector3.zero, 11f, 1.6f);
    }

    // ================================================================== OBSTACLES

    static void AddTrigger(GameObject go, Vector3 center, Vector3 size, ObstacleKind kind)
    {
        BoxCollider bc = go.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.center = center;
        bc.size = size;
        go.AddComponent<Obstacle>().kind = kind;
    }

    /// <summary>Beach tram (full-height obstacle): change lanes.</summary>
    public static GameObject Train(Transform parent, Vector3 pos, int colorIdx)
    {
        Transform root = NewRoot("BeachTram", parent, pos);
        Color[] bodies = { C(255, 105, 170), C(40, 205, 195), C(255, 205, 70), C(245, 240, 235), C(170, 120, 255) };
        Material mb = Mats.Solid(bodies[colorIdx % bodies.Length], 0.55f);
        Material white = Mats.Solid(C(250, 248, 244), 0.4f);
        Material glass = Mats.Solid(C(25, 35, 70), 0.9f);
        Material neon = Mats.Neon(GameConfig.NeonColors[colorIdx % 5]);

        Prim.Box(root, new Vector3(0f, 1.55f, 0f), new Vector3(2.1f, 2.7f, 7f), mb, true);
        Prim.Box(root, new Vector3(0f, 0.45f, 0f), new Vector3(2.14f, 0.5f, 7.04f), white, true);
        Prim.Box(root, new Vector3(0f, 2.1f, 0f), new Vector3(2.16f, 0.9f, 5.8f), glass);
        Prim.Box(root, new Vector3(0f, 1.3f, 0f), new Vector3(2.13f, 0.1f, 7.02f), neon);
        Prim.Box(root, new Vector3(0f, 3.05f, 0f), new Vector3(2.0f, 0.25f, 6.8f), white, true);
        Prim.Box(root, new Vector3(0f, 3.28f, 1.5f), new Vector3(1.0f, 0.3f, 1.2f), Mats.Solid(C(200, 205, 215), 0.4f));
        // surfboards on the roof
        Prim.Box(root, new Vector3(-0.4f, 3.25f, -1.0f), new Vector3(0.4f, 0.08f, 2.3f), Mats.Solid(GameConfig.Cyan, 0.6f));
        Prim.Box(root, new Vector3(0.4f, 3.25f, -1.2f), new Vector3(0.4f, 0.08f, 2.1f), Mats.Solid(GameConfig.Orange, 0.6f));
        // front (faces the player, -Z)
        Prim.Box(root, new Vector3(0f, 2.1f, -3.51f), new Vector3(1.7f, 1.0f, 0.05f), glass);
        Prim.Sphere(root, new Vector3(-0.7f, 0.9f, -3.52f), 0.3f, Mats.Neon(C(255, 250, 200)));
        Prim.Sphere(root, new Vector3(0.7f, 0.9f, -3.52f), 0.3f, Mats.Neon(C(255, 250, 200)));

        AddTrigger(root.gameObject, new Vector3(0f, 1.55f, 0f), new Vector3(1.95f, 3.0f, 6.9f), ObstacleKind.Train);
        return root.gameObject;
    }

    /// <summary>Striped beach barrier (low obstacle): jump over it.</summary>
    public static GameObject Barrier(Transform parent, Vector3 pos)
    {
        Transform root = NewRoot("BeachBarrier", parent, pos);
        Material wood = Mats.Solid(C(250, 245, 238), 0.3f);
        Material red = Mats.Solid(C(255, 60, 90), 0.3f);
        Material wht = Mats.Solid(C(255, 255, 255), 0.3f);
        Prim.Box(root, new Vector3(-1.0f, 0.55f, 0f), new Vector3(0.14f, 1.1f, 0.14f), wood, true);
        Prim.Box(root, new Vector3(1.0f, 0.55f, 0f), new Vector3(0.14f, 1.1f, 0.14f), wood, true);
        for (int i = 0; i < 3; i++)
            Prim.Box(root, new Vector3(0f, 0.25f + i * 0.28f, 0f), new Vector3(2.2f, 0.24f, 0.12f), (i % 2 == 0) ? red : wht, true);
        Prim.Sphere(root, new Vector3(-1.0f, 1.15f, 0f), 0.2f, Mats.Neon(GameConfig.Orange));
        Prim.Sphere(root, new Vector3(1.0f, 1.15f, 0f), 0.2f, Mats.Neon(GameConfig.Orange));
        AddTrigger(root.gameObject, new Vector3(0f, 0.5f, 0f), new Vector3(2.2f, 1.0f, 0.6f), ObstacleKind.Barrier);
        return root.gameObject;
    }

    /// <summary>Neon overhead sign (high obstacle): slide under it.</summary>
    public static GameObject SignBoard(Transform parent, Vector3 pos, int textIdx)
    {
        Transform root = NewRoot("NeonSign", parent, pos);
        Material post = Mats.Solid(C(60, 50, 90), 0.5f);
        Color neon = GameConfig.NeonColors[textIdx % 5];
        Prim.Box(root, new Vector3(-1.15f, 1.4f, 0f), new Vector3(0.15f, 2.8f, 0.15f), post, true);
        Prim.Box(root, new Vector3(1.15f, 1.4f, 0f), new Vector3(0.15f, 2.8f, 0.15f), post, true);
        Prim.Box(root, new Vector3(0f, 1.65f, 0f), new Vector3(2.5f, 0.9f, 0.18f), Mats.Solid(GameConfig.Navy, 0.5f), true);
        Prim.Box(root, new Vector3(0f, 2.1f, -0.02f), new Vector3(2.56f, 0.07f, 0.2f), Mats.Neon(neon));
        Prim.Box(root, new Vector3(0f, 1.2f, -0.02f), new Vector3(2.56f, 0.07f, 0.2f), Mats.Neon(neon));
        Prim.Box(root, new Vector3(-1.26f, 1.65f, -0.02f), new Vector3(0.07f, 0.9f, 0.2f), Mats.Neon(neon));
        Prim.Box(root, new Vector3(1.26f, 1.65f, -0.02f), new Vector3(0.07f, 0.9f, 0.2f), Mats.Neon(neon));
        WorldText.Create(root, SignNames[textIdx % SignNames.Length], neon, new Vector3(0f, 1.65f, -0.12f), Vector3.zero, 2.3f, 0.7f);
        AddTrigger(root.gameObject, new Vector3(0f, 1.65f, 0f), new Vector3(2.4f, 0.9f, 0.5f), ObstacleKind.SignBoard);
        return root.gameObject;
    }

    // ================================================================== COLLECTIBLES

    public static GameObject Coin(Transform parent, Vector3 pos)
    {
        Transform root = NewRoot("Coin", parent, pos);
        SphereCollider sc = root.gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.6f;
        Prim.Rot(Prim.Cyl(root, Vector3.zero, 0.55f, 0.07f, Mats.Solid(C(255, 205, 40), 0.7f, 0.25f)), new Vector3(90f, 0f, 0f));
        Prim.Rot(Prim.Cyl(root, new Vector3(0f, 0f, 0.012f), 0.62f, 0.05f, Mats.Solid(C(200, 130, 20), 0.5f)), new Vector3(90f, 0f, 0f));
        Prim.Rot(Prim.Cyl(root, Vector3.zero, 0.3f, 0.09f, Mats.Solid(C(255, 235, 120), 0.7f, 0.25f)), new Vector3(90f, 0f, 0f));
        root.gameObject.AddComponent<CollectableRotate>();
        root.gameObject.AddComponent<CoinCollection>();
        return root.gameObject;
    }

    public static GameObject Key(Transform parent, Vector3 pos)
    {
        Transform root = NewRoot("Key", parent, pos);
        SphereCollider sc = root.gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.7f;
        Transform model = Prim.Pivot("Model", root, Vector3.zero);
        model.localRotation = Quaternion.Euler(0f, 0f, 25f);
        Material mat = Mats.Solid(C(0, 225, 255), 0.8f, 0.3f);
        Prim.Box(model, new Vector3(0f, -0.05f, 0f), new Vector3(0.1f, 0.6f, 0.08f), mat);
        Prim.Rot(Prim.Cyl(model, new Vector3(0f, 0.42f, 0f), 0.38f, 0.08f, mat), new Vector3(90f, 0f, 0f));
        Prim.Rot(Prim.Cyl(model, new Vector3(0f, 0.42f, 0f), 0.16f, 0.1f, Mats.Solid(GameConfig.Navy, 0.5f)), new Vector3(90f, 0f, 0f));
        Prim.Box(model, new Vector3(0.1f, -0.28f, 0f), new Vector3(0.16f, 0.09f, 0.08f), mat);
        Prim.Box(model, new Vector3(0.1f, -0.10f, 0f), new Vector3(0.16f, 0.09f, 0.08f), mat);
        root.gameObject.AddComponent<CollectableRotate>();
        root.gameObject.AddComponent<KeyCollection>();
        return root.gameObject;
    }
}
