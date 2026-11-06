using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One 60 m piece of the Miami boardwalk: ground, scenery, obstacle rows and coins/keys.
/// Obstacle rows use a "safe lane" that never jumps more than one lane, so every layout is passable.
/// </summary>
public class TrackSegment : MonoBehaviour
{
    public float startZ;
    public float EndZ { get { return startZ + GameConfig.SegmentLength; } }

    readonly List<GameObject> obstacles = new List<GameObject>();

    static readonly float[] RowZ = { 8f, 22f, 36f, 50f };

    public void Build(int index, bool obstaclesOn, ref int lastSafe)
    {
        WorldBuilder.Ground(transform);

        Transform scenery = new GameObject("Scenery").transform;
        scenery.SetParent(transform, false);
        WorldBuilder.Scenery(scenery, index);

        Transform play = new GameObject("Gameplay").transform;
        play.SetParent(transform, false);

        bool placeObstacles = obstaclesOn && index >= 2;
        if (!placeObstacles)
        {
            if (index >= 1 || !obstaclesOn) FreeCoinRuns(play);
            return;
        }
        BuildObstacleRows(play, index, ref lastSafe);
    }

    void FreeCoinRuns(Transform play)
    {
        for (int run = 0; run < 3; run++)
        {
            int lane = Random.Range(0, 3);
            float z0 = 6f + run * 18f;
            for (int i = 0; i < 6; i++)
                WorldBuilder.Coin(play, new Vector3((lane - 1) * GameConfig.LaneDistance, 1.0f, z0 + i * 2.2f));
        }
    }

    void BuildObstacleRows(Transform play, int index, ref int lastSafe)
    {
        float density = Mathf.Clamp(0.25f + (index - 2) * 0.025f, 0.25f, 0.78f);
        int[] safe = new int[RowZ.Length];
        int[] safeKind = new int[RowZ.Length];   // -1 none, 1 barrier, 2 sign

        for (int r = 0; r < RowZ.Length; r++)
        {
            int s = Mathf.Clamp(lastSafe + Random.Range(-1, 2), 0, 2);
            safe[r] = s;
            safeKind[r] = -1;
            lastSafe = s;
            float z = RowZ[r];

            for (int lane = 0; lane < 3; lane++)
            {
                float x = (lane - 1) * GameConfig.LaneDistance;
                int kind = -1;
                if (lane == s)
                {
                    if (Random.value < 0.10f + density * 0.20f)
                    {
                        kind = Random.value < 0.5f ? 1 : 2;
                        safeKind[r] = kind;
                    }
                }
                else if (Random.value < 0.35f + density * 0.5f)
                {
                    float rv = Random.value;
                    kind = rv < 0.45f ? 0 : (rv < 0.75f ? 1 : 2);
                }
                if (kind < 0) continue;

                GameObject o;
                Vector3 p = new Vector3(x, 0f, z);
                if (kind == 0) o = WorldBuilder.Train(play, p, Random.Range(0, 5));
                else if (kind == 1) o = WorldBuilder.Barrier(play, p);
                else o = WorldBuilder.SignBoard(play, p, Random.Range(0, 8));
                obstacles.Add(o);
            }
        }

        bool keyPlaced = false;
        for (int r = 0; r < RowZ.Length; r++)
        {
            float z = RowZ[r];
            float lx = (safe[r] - 1) * GameConfig.LaneDistance;

            // reward for the skill obstacle in the safe lane
            if (safeKind[r] == 1)
            {
                for (int i = -2; i <= 2; i++)
                {
                    float y = 0.9f + 0.8f * (1f - (i / 2.2f) * (i / 2.2f));
                    WorldBuilder.Coin(play, new Vector3(lx, y, z + i));
                }
            }
            else if (safeKind[r] == 2)
            {
                for (int i = -1; i <= 1; i++) WorldBuilder.Coin(play, new Vector3(lx, 0.6f, z + i * 1.5f));
            }

            // coin run in the gap after this row (towards the next safe lane)
            int nextLane = (r < RowZ.Length - 1) ? safe[r + 1] : safe[r];
            float nx = (nextLane - 1) * GameConfig.LaneDistance;
            if (!keyPlaced && r == 1 && Random.value < 0.28f)
            {
                WorldBuilder.Key(play, new Vector3(nx, 1.2f, z + 7.5f));
                keyPlaced = true;
            }
            else
            {
                for (int i = 0; i < 5; i++)
                    WorldBuilder.Coin(play, new Vector3(nx, 1.0f, z + 4f + i * 1.6f));
            }
        }
    }

    public void ClearObstacles(float zFrom, float zTo)
    {
        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            GameObject o = obstacles[i];
            if (o == null) { obstacles.RemoveAt(i); continue; }
            float z = o.transform.position.z;
            if (z > zFrom && z < zTo)
            {
                Destroy(o);
                obstacles.RemoveAt(i);
            }
        }
    }
}
