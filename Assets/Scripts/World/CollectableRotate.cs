using UnityEngine;

/// <summary>Spins coins and keys around the world Y axis so the player can spot them.</summary>
public class CollectableRotate : MonoBehaviour
{
    [SerializeField] float rotateSpeed = 150f;   // degrees per second

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
    }
}
