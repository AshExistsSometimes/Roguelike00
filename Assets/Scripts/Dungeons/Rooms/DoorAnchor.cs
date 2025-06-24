using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnchor : MonoBehaviour
{
    // ENSURE POSITIVE Z POINTS OUT OF THE DOOR
    public bool isConnected = false;

    public Vector3 Position => transform.position;
    public Vector3 Forward => transform.forward;

    public DoorAnchor connectedTo; // set in DungeonGenerator - TryAttachRoom

    public void Connect(DoorAnchor other)
    {
        if (other == null) return;

        this.connectedTo = other;
        this.isConnected = true;

        other.connectedTo = this;
        other.isConnected = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isConnected ? Color.green : Color.red;

        // Position dot
        Gizmos.DrawSphere(transform.position, 0.15f);

        // Direction arrow
        Gizmos.color = Color.yellow;
        Vector3 direction = transform.forward * 0.5f;
        Gizmos.DrawLine(transform.position, transform.position + direction);
        Gizmos.DrawRay(transform.position, direction);
    }
}
