using UnityEngine;

/// <summary>
/// Spawns a fuse box with 6 coloured wire stubs on the elevator back wall.
/// Each wire is an interactable GameObject tagged "Wire" with a WireButton
/// component that tells SimonSaysGame which colour was clicked.
///
/// SETUP:
///   1. Attach to any persistent GameObject in the Floor 3 scene.
///   2. Position the spawner at the elevator's back wall centre.
///   3. Right-click → "Spawn Wire Panel".
///   4. Assign your player raycast script to look for tag "Wire".
/// </summary>
public class WirePanel : MonoBehaviour
{
    [Header("Panel Position")]
    [Tooltip("Offset from this transform — place on the back wall face")]
    public Vector3 panelOffset = new Vector3(0f, 2.5f, 0f);

    [Header("Materials")]
    public Material boxMaterial;      // dark metal for the fuse box
    public Material wireMaterial;     // fallback if no per-colour material

    // Wire colours — matches reference screenshot order
    private static readonly Color[] WireColors = new Color[]
    {
        new Color(0.85f, 0.15f, 0.15f),   // 0 Red
        new Color(0.15f, 0.45f, 0.85f),   // 1 Blue
        new Color(0.15f, 0.75f, 0.25f),   // 2 Green
        new Color(0.9f,  0.8f,  0.1f),    // 3 Yellow
        new Color(0.6f,  0.15f, 0.8f),    // 4 Purple
        new Color(0.85f, 0.45f, 0.1f),    // 5 Orange
    };

    public static readonly string[] ColorNames = new string[]
    { "Red", "Blue", "Green", "Yellow", "Purple", "Orange" };

    private GameObject _root;

    [ContextMenu("Spawn Wire Panel")]
    public void SpawnWirePanel()
    {
        Transform existing = transform.Find("WirePanel_Root");
        if (existing != null) DestroyImmediate(existing.gameObject);
        if (_root    != null) DestroyImmediate(_root);

        _root = new GameObject("WirePanel_Root");
        _root.transform.SetParent(transform, false);
        _root.transform.localPosition = panelOffset;

        // Fuse box backing plate
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "FuseBox";
        box.transform.SetParent(_root.transform, false);
        box.transform.localPosition = Vector3.zero;
        box.transform.localScale    = new Vector3(2.8f, 1.2f, 0.12f);
        if (boxMaterial != null)
            box.GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;
        else
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color = new Color(0.12f, 0.12f, 0.12f);
            box.GetComponent<MeshRenderer>().material = m;
        }

        // 6 wire stubs spaced evenly along the box
        float spacing  = 2.4f / 5f;
        float startX   = -1.2f;

        for (int i = 0; i < 6; i++)
        {
            float x = startX + spacing * i;

            // Wire cylinder (the "stub" poking out of the box)
            GameObject wire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wire.name = $"Wire_{ColorNames[i]}";
            wire.transform.SetParent(_root.transform, false);
            wire.transform.localPosition = new Vector3(x, 0f, -0.14f);
            wire.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            wire.transform.localScale    = new Vector3(0.12f, 0.18f, 0.12f);
            wire.tag = "Wire";

            // Colour material
            var mr  = wire.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = WireColors[i];
            mr.material = mat;

            // WireButton component — stores which index this wire is
            WireButton wb = wire.AddComponent<WireButton>();
            wb.wireIndex  = i;
            wb.colorName  = ColorNames[i];

            // Collider is already added by CreatePrimitive
            // Make sure it's not a trigger (player raycast needs solid hit)
            wire.GetComponent<Collider>().isTrigger = false;

            // Small label cube underneath each wire (the socket)
            GameObject socket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            socket.name = "Socket";
            socket.transform.SetParent(_root.transform, false);
            socket.transform.localPosition = new Vector3(x, -0.3f, -0.07f);
            socket.transform.localScale    = new Vector3(0.16f, 0.16f, 0.1f);
            var sm  = socket.GetComponent<MeshRenderer>();
            var smt = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            smt.color = new Color(0.08f, 0.08f, 0.08f);
            sm.material = smt;
        }

        Debug.Log("[WirePanel] Spawned 6 wires.");
    }
}