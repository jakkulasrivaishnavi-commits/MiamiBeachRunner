using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Endless track generation (PPT "SegmentGenerator"): keeps segments spawned ahead of the target
/// and removes the ones left behind. Segment length is 60 m.
/// </summary>
public class SegmentGenerator : MonoBehaviour
{
    public Transform target;
    public bool withObstacles = true;
    public float spawnAhead = 210f;
    public float despawnBehind = 80f;
    public float startZ = -60f;

    readonly List<TrackSegment> segments = new List<TrackSegment>();
    float nextZ;
    int nextIndex;
    int lastSafeLane = 1;

    void Start()
    {
        nextZ = startZ;
        if (target == null) return;
        while (nextZ < target.position.z + spawnAhead) SpawnOne();
    }

    void Update()
    {
        if (target == null) return;
        if (nextZ < target.position.z + spawnAhead) SpawnOne();   // at most one per frame

        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (segments[i] == null) { segments.RemoveAt(i); continue; }
            if (segments[i].EndZ < target.position.z - despawnBehind)
            {
                Destroy(segments[i].gameObject);
                segments.RemoveAt(i);
            }
        }
    }

    void SpawnOne()
    {
        GameObject go = new GameObject("Segment_" + nextIndex);
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(0f, 0f, nextZ);
        TrackSegment seg = go.AddComponent<TrackSegment>();
        seg.startZ = nextZ;
        seg.Build(nextIndex, withObstacles, ref lastSafeLane);
        segments.Add(seg);
        nextZ += GameConfig.SegmentLength;
        nextIndex++;
    }

    /// <summary>Used by Revive: removes obstacles in a z-range so the player can continue safely.</summary>
    public void ClearObstacles(float zFrom, float zTo)
    {
        foreach (TrackSegment s in segments)
            if (s != null) s.ClearObstacles(zFrom, zTo);
    }
}
