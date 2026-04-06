using UnityEngine;

/// <summary>
/// Spawns a simple elevator box for Floor 3 — Circuit Recovery.
/// Matches the dimensions of the previous elevator cars.
/// Right-click → "Spawn Elevator" to build in Editor.
/// </summary>
public class Floor3ElevatorSpawner : MonoBehaviour
{
    [Header("Elevator Dimensions")]
    public float carWidth  = 10f;
    public float carDepth  = 6f;
    public float carHeight = 6f;
    public float wallThick = 0.2f;

    [Header("Materials")]
    public Material wallMaterial;
    public Material floorMaterial;
    public Material ceilingMaterial;

    private GameObject _root;

    [ContextMenu("Spawn Elevator")]
    public void SpawnElevator()
    {
        // Clear previous
        Transform existing = transform.Find("Floor3_Elevator");
        if (existing != null) DestroyImmediate(existing.gameObject);
        if (_root    != null) DestroyImmediate(_root);

        _root      = new GameObject("Floor3_Elevator");
        _root.transform.SetParent(transform, false);
        _root.transform.localPosition = Vector3.zero;

        float T   = wallThick;
        float mid = carHeight * 0.5f;
        float cx  = 0f;
        float cz  = carDepth * 0.5f;   // car extends in +Z from origin

        // Floor
        Slab("Car_Floor",
            new Vector3(cx, -T * 0.5f, cz),
            new Vector3(carWidth, T, carDepth),
            floorMaterial);

        // Ceiling
        Slab("Car_Ceiling",
            new Vector3(cx, carHeight + T * 0.5f, cz),
            new Vector3(carWidth, T, carDepth),
            ceilingMaterial);

        // Back wall (+Z)
        Slab("Car_WallBack",
            new Vector3(cx, mid, carDepth + T * 0.5f),
            new Vector3(carWidth, carHeight, T),
            wallMaterial);

        // Left wall (-X)
        Slab("Car_WallLeft",
            new Vector3(-carWidth * 0.5f - T * 0.5f, mid, cz),
            new Vector3(T, carHeight, carDepth),
            wallMaterial);

        // Right wall (+X)
        Slab("Car_WallRight",
            new Vector3(carWidth * 0.5f + T * 0.5f, mid, cz),
            new Vector3(T, carHeight, carDepth),
            wallMaterial);

        // Front wall (Z=0 side) — open entrance, just a header above door height
        float doorH  = carHeight * 0.75f;
        float aboveH = carHeight - doorH;
        if (aboveH > 0.01f)
            Slab("Car_WallFront_Header",
                new Vector3(cx, doorH + aboveH * 0.5f, -T * 0.5f),
                new Vector3(carWidth, aboveH, T),
                wallMaterial);

        // Two side strips flanking the door opening on front wall
        float openingW = carWidth * 0.55f;
        float stripW   = (carWidth - openingW) * 0.5f;
        if (stripW > 0.01f)
        {
            Slab("Car_WallFront_L",
                new Vector3(-openingW * 0.5f - stripW * 0.5f, mid, -T * 0.5f),
                new Vector3(stripW, carHeight, T),
                wallMaterial);

            Slab("Car_WallFront_R",
                new Vector3(openingW * 0.5f + stripW * 0.5f, mid, -T * 0.5f),
                new Vector3(stripW, carHeight, T),
                wallMaterial);
        }

        Debug.Log("[Floor3ElevatorSpawner] Elevator spawned.");
    }

    GameObject Slab(string objName, Vector3 localPos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.SetParent(_root.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = scale;
        if (mat != null)
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
#if UNITY_EDITOR
        UnityEditor.GameObjectUtility.SetStaticEditorFlags(go,
            UnityEditor.StaticEditorFlags.ContributeGI   |
            UnityEditor.StaticEditorFlags.BatchingStatic |
            UnityEditor.StaticEditorFlags.OccluderStatic |
            UnityEditor.StaticEditorFlags.OccludeeStatic);
#endif
        return go;
    }
}