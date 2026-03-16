using UnityEngine;

/// <summary>
/// Standalone elevator room builder — no OfficeGenerator needed.
///
/// SETUP:
///   1. Create an empty GameObject at the entrance of your elevator area.
///   2. Add this component (ElevatorController is added automatically).
///   3. Assign materials in the Inspector.
///   4. Right-click the component → "1 — Snap Parent to Floor" first,
///      then → "2 — Build Elevator Room".
///   5. Doors, call button, and close button are wired automatically.
/// </summary>
public class ElevatorRoomBuilder : MonoBehaviour
{
    [Header("Room Dimensions")]
    [Tooltip("Total width of the elevator lobby along X")]
    public float roomWidth  = 8f;
    [Tooltip("Depth of the lobby along Z (entrance → back wall)")]
    public float roomDepth  = 6f;
    [Tooltip("Ceiling height")]
    public float roomHeight = 3f;
    public float wallThick  = 0.2f;

    [Header("Elevator Car")]
    public float carWidth = 2.8f;
    public float carDepth = 2.8f;

    [Header("Sliding Doors")]
    [Tooltip("Width of EACH door panel — two panels total cover the opening")]
    public float doorPanelWidth = 1.3f;
    [Tooltip("Height of the door opening")]
    public float doorHeight     = 2.4f;

    [Header("Materials")]
    public Material wallMaterial;
    public Material floorMaterial;
    public Material ceilingMaterial;
    public Material glassMaterial;
    public Material buttonMaterial;

    private const float DOOR_THICK = 0.05f;
    private const float FRAME_W    = 0.05f;

    // ──────────────────────────────────────────────────────────────────────

    [ContextMenu("1 — Snap Parent to Floor (run first)")]
    public void SnapToFloor()
    {
        Vector3 p = transform.position;
        p.y = 0f;
        transform.position = p;
        Debug.Log("[ElevatorRoomBuilder] Y snapped to 0 (world floor).");
    }

    [ContextMenu("2 — Build Elevator Room")]
    public void BuildElevatorRoom()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        BuildElevatorCar();
        BuildDoorsAndButtons();

        Debug.Log("[ElevatorRoomBuilder] Done. ElevatorController wired.");
    }

    // ── Lobby shell ────────────────────────────────────────────────────────
    void BuildLobbyShell()
    {
        // Lobby removed — car only
    }

    // ── Elevator car interior ──────────────────────────────────────────────
    void BuildElevatorCar()
    {
        float T          = wallThick;
        float carZCentre = roomDepth - carDepth * 0.5f - T;
        float mid        = roomHeight * 0.5f;
        float carFrontZ  = roomDepth - carDepth - T;
        float aboveH     = roomHeight - doorHeight;

        // Floor
        Static("Car_Floor",
            new Vector3(0f, -T * 0.5f, carZCentre),
            new Vector3(carWidth, T, carDepth),
            floorMaterial);

        // Ceiling
        Static("Car_Ceiling",
            new Vector3(0f, roomHeight + T * 0.5f, carZCentre),
            new Vector3(carWidth, T, carDepth),
            ceilingMaterial);

        // Left wall
        Static("Car_WallLeft",
            new Vector3(-carWidth * 0.5f - T * 0.5f, mid, carZCentre),
            new Vector3(T, roomHeight, carDepth),
            wallMaterial);

        // Right wall
        Static("Car_WallRight",
            new Vector3(carWidth * 0.5f + T * 0.5f, mid, carZCentre),
            new Vector3(T, roomHeight, carDepth),
            wallMaterial);

        // Back wall
        Static("Car_WallBack",
            new Vector3(0f, mid, roomDepth - T * 0.5f),
            new Vector3(carWidth, roomHeight, T),
            wallMaterial);

        // Front wall above door opening
        if (aboveH > 0.01f)
            Static("Car_WallFront_Header",
                new Vector3(0f, doorHeight + aboveH * 0.5f, carFrontZ - T * 0.5f),
                new Vector3(carWidth, aboveH, T),
                wallMaterial);
    }

    // ── Doors + buttons ────────────────────────────────────────────────────
    void BuildDoorsAndButtons()
    {
        float T     = wallThick;
        // Doors sit at the ENTRANCE — Z=0 is the hallway-facing front of the lobby.
        float doorZ = T * 0.5f;

        // Header track rail across the full opening at entrance
        Static("Door_Track",
            new Vector3(0f, doorHeight + FRAME_W * 0.5f, doorZ),
            new Vector3(carWidth + T, FRAME_W, DOOR_THICK + 0.02f),
            wallMaterial);

        // Left panel — slides left (-X) to open
        GameObject leftPanel  = MakeDoorPanel("Door_Left",
            new Vector3(-doorPanelWidth * 0.5f, doorHeight * 0.5f, doorZ));

        // Right panel — slides right (+X) to open
        GameObject rightPanel = MakeDoorPanel("Door_Right",
            new Vector3(doorPanelWidth * 0.5f, doorHeight * 0.5f, doorZ));

        // Call button — on the right side wall just outside the entrance
        GameObject callGo = MakeButton("Button_Call",
            new Vector3(carWidth * 0.5f + T + 0.06f, 1.1f, doorZ + 0.3f),
            new Color(0.2f, 0.8f, 0.2f),
            requireKey: false);

        // Close button — on the car's left inner wall at waist height
        GameObject closeGo = MakeButton("Button_Close",
            new Vector3(-carWidth * 0.5f + 0.15f, 1.1f, roomDepth - carDepth * 0.5f),
            new Color(0.9f, 0.2f, 0.2f),
            requireKey: false);

        // Wire ElevatorController — add it if it isn't already present
        ElevatorController ctrl = GetComponent<ElevatorController>();
        if (ctrl == null) ctrl = gameObject.AddComponent<ElevatorController>();
        ctrl.leftDoor      = leftPanel.transform;
        ctrl.rightDoor     = rightPanel.transform;
        ctrl.callButton    = callGo.GetComponent<InteractableButton>();
        ctrl.closeButton   = closeGo.GetComponent<InteractableButton>();
        ctrl.slideDistance = doorPanelWidth * 0.95f;
    }

    // ── Door panel factory — NOT static so it can move at runtime ──────────
    GameObject MakeDoorPanel(string objName, Vector3 localPos)
    {
        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = objName;
        panel.transform.SetParent(transform, false);
        panel.transform.localPosition = localPos;
        panel.transform.localScale    = new Vector3(doorPanelWidth, doorHeight, DOOR_THICK);
        ApplyMat(panel, glassMaterial != null ? glassMaterial : wallMaterial);
        // No SetStatic — must be movable

        // Metal edge frame strips (children, also non-static)
        // localScale components are fractional (0–1) because parent sets real size
        MakeStrip("Frame_Top",    panel, new Vector3(0f,  0.5f - FRAME_W / doorHeight * 0.5f, 0f),
                  new Vector3(1f, FRAME_W / doorHeight, 1.4f));
        MakeStrip("Frame_Bottom", panel, new Vector3(0f, -0.5f + FRAME_W / doorHeight * 0.5f, 0f),
                  new Vector3(1f, FRAME_W / doorHeight, 1.4f));
        MakeStrip("Frame_Left",   panel, new Vector3(-0.5f + FRAME_W / doorPanelWidth * 0.5f, 0f, 0f),
                  new Vector3(FRAME_W / doorPanelWidth, 1f, 1.4f));
        MakeStrip("Frame_Right",  panel, new Vector3( 0.5f - FRAME_W / doorPanelWidth * 0.5f, 0f, 0f),
                  new Vector3(FRAME_W / doorPanelWidth, 1f, 1.4f));

        return panel;
    }

    void MakeStrip(string objName, GameObject parent, Vector3 lpos, Vector3 lscale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = lpos;
        go.transform.localScale    = lscale;
        ApplyMat(go, wallMaterial);
    }

    // ── Button factory ─────────────────────────────────────────────────────
    GameObject MakeButton(string objName, Vector3 localPos, Color pressedCol, bool requireKey)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = new Vector3(0.12f, 0.12f, 0.08f);
        ApplyMat(go, buttonMaterial);
        go.tag = "Interactable";

        InteractableButton btn = go.AddComponent<InteractableButton>();
        btn.requiresKey  = requireKey;
        btn.pressedColor = pressedCol;
        return go;
    }

    // ── Static slab factory ────────────────────────────────────────────────
    GameObject Static(string objName, Vector3 localPos, Vector3 scale, Material mat = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = scale;
        ApplyMat(go, mat ?? wallMaterial);
        SetStatic(go);
        return go;
    }

    void ApplyMat(GameObject go, Material mat)
    {
        if (mat == null) return;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr) mr.sharedMaterial = mat;
    }

    void SetStatic(GameObject go)
    {
#if UNITY_EDITOR
        UnityEditor.GameObjectUtility.SetStaticEditorFlags(go,
            UnityEditor.StaticEditorFlags.ContributeGI   |
            UnityEditor.StaticEditorFlags.BatchingStatic |
            UnityEditor.StaticEditorFlags.OccluderStatic |
            UnityEditor.StaticEditorFlags.OccludeeStatic);
#endif
    }
}