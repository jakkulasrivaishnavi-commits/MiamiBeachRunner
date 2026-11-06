using UnityEngine;

public enum ObstacleKind { Train, Barrier, SignBoard }

/// <summary>Marker placed on every obstacle trigger. CollisionDetect looks for this component.</summary>
public class Obstacle : MonoBehaviour
{
    public ObstacleKind kind;
}
