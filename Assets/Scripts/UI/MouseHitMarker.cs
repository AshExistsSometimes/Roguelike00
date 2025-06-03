using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHitMarker : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject markerPrefab;
    private GameObject currentMarker;

    void Update()
    {
        if (!mainCamera || !markerPrefab)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (currentMarker == null)
            {
                currentMarker = Instantiate(markerPrefab);
            }

            currentMarker.transform.position = hit.point;
            currentMarker.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
        else
        {
            if (currentMarker != null)
            {
                Destroy(currentMarker);
                currentMarker = null;
            }
        }
    }
}
