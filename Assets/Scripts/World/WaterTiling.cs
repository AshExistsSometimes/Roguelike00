using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTiling : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject waterTilePrefab;
    public float tileSize = 10f;

    [Header("Tiling Limits")]
    public int tilesX = 10;
    public int tilesZ = 10;

    [Header("Gizmo Settings")]
    public Color boundsColor = new Color(0f, 0.5f, 1f, 0.2f);

    private void Start()
    {
        GenerateTiles();
    }

    private void GenerateTiles()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        Vector3 startPosition = transform.position;

        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                Vector3 worldPosition = startPosition + new Vector3(x * tileSize, 0f, z * tileSize);
                GameObject tile = Instantiate(waterTilePrefab, worldPosition, Quaternion.identity, transform);
                tile.name = $"WaterTile_{x}_{z}";
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = boundsColor;
        Vector3 gizmoCenter = transform.position + new Vector3(
            (tilesX * tileSize) / 2f - tileSize / 2f,
            0f,
            (tilesZ * tileSize) / 2f - tileSize / 2f
        );

        Gizmos.DrawCube(
            gizmoCenter,
            new Vector3(tilesX * tileSize, 0.1f, tilesZ * tileSize)
        );
    }
}

