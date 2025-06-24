using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInstance : MonoBehaviour
{
    public List<DoorAnchor> Doors { get; private set; } = new List<DoorAnchor>();

    void Awake()
    {
        Doors = new List<DoorAnchor>(GetComponentsInChildren<DoorAnchor>());
    }

    public DoorAnchor GetUnconnectedDoor()
    {
        return Doors.Find(d => !d.isConnected);
    }

    public List<DoorAnchor> GetUnconnectedDoors()
    {
        var unconnected = Doors.FindAll(d => !d.isConnected);

        Debug.Log($"[RoomInstance:{name}] Total doors: {Doors.Count}, Unconnected doors: {unconnected.Count}");
        foreach (var door in Doors)
        {
            Debug.Log($"  Door '{door.name}': isConnected = {door.isConnected}");
        }

        return unconnected;
    }

    ///
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        var bounds = CalculateBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    private Bounds CalculateBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);
        return bounds;
    }

    public Bounds Bounds
    {
        get
        {
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }
    }
}
