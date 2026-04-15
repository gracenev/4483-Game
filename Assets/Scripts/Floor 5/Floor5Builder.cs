using UnityEngine;

// Generates the Floor 5 office layout — a small contained floor with
// a main hallway, 3 side rooms, a server room, and an elevator bay.
// Right-click -> "Build Floor 5" to generate.
// Right-click -> "Clear Floor 5" to remove.
public class Floor5Builder : MonoBehaviour
{
    [Header("Overall Dimensions")]
    public float hallwayLength = 40f;
    public float hallwayWidth  = 8f;
    public float wallHeight    = 6f;
    public float wallThick     = 0.2f;

    [Header("Side Rooms")]
    public float roomWidth     = 8f;    // how far rooms extend sideways
    public float roomDepth     = 8f;    // depth of each room along hallway
    public float doorWidth     = 2.5f;
    public float doorHeight    = 4f;

    [Header("Server Room (end of hallway left side)")]
    public float serverRoomWidth = 8f;
    public float serverRoomDepth = 10f;

    [Header("Elevator Bay (far end)")]
    public float elevatorBayDepth = 8f;

    [Header("Materials")]
    public Material floorMaterial;
    public Material wallMaterial;
    public Material ceilingMaterial;
    public Material doorMaterial;
    public Material glassMaterial;
    public Material panelFrameMaterial;
    public Material panelDiffuserMaterial;

    [Header("Lighting")]
    public float lightRange     = 8f;
    public float lightIntensity = 1.5f;
    public Color normalLightColor = new Color(1f, 0.95f, 0.85f);  // warm white

    private GameObject _root;
    private const float T = 0.2f;

    [ContextMenu("Build Floor 5")]
    public void Build()
    {
        Clear();
        _root = new GameObject("Floor5_Root");
        _root.transform.SetParent(transform, false);

        BuildHallway();
        BuildRooms();
        BuildServerRoom();
        BuildLights();
        Debug.Log($"[Floor5Builder] Elevator attaches at Z={hallwayLength}, centred on X=0. Hallway end is open — just position your existing elevator room there.");
    }

    [ContextMenu("Clear Floor 5")]
    public void Clear()
    {
        Transform ex = transform.Find("Floor5_Root");
        if (ex    != null) DestroyImmediate(ex.gameObject);
        if (_root != null) DestroyImmediate(_root);
    }

    // ── Main hallway — runs along Z axis ──────────────────────────────────
    void BuildHallway()
    {
        GameObject hall = new GameObject("Hallway");
        hall.transform.SetParent(_root.transform, false);

        float mid = wallHeight * 0.5f;
        float cx  = 0f;
        float cz  = hallwayLength * 0.5f;

        // Floor
        Slab(hall, "Hall_Floor",
            new Vector3(cx, -T * 0.5f, cz),
            new Vector3(hallwayWidth, T, hallwayLength),
            floorMaterial, true);

        // Ceiling
        Slab(hall, "Hall_Ceiling",
            new Vector3(cx, wallHeight + T * 0.5f, cz),
            new Vector3(hallwayWidth, T, hallwayLength),
            ceilingMaterial, true);

        // Back wall — player entrance
        Slab(hall, "Hall_WallBack",
            new Vector3(cx, mid, -T * 0.5f),
            new Vector3(hallwayWidth, wallHeight, T),
            wallMaterial, true);

        // Left hallway wall — full length, rooms punch their own door openings into it
        Slab(hall, "Hall_WallLeft",
            new Vector3(-hallwayWidth * 0.5f - T * 0.5f, mid, cz),
            new Vector3(T, wallHeight, hallwayLength),
            wallMaterial, true);

        // Right hallway wall — full length
        Slab(hall, "Hall_WallRight",
            new Vector3(hallwayWidth * 0.5f + T * 0.5f, mid, cz),
            new Vector3(T, wallHeight, hallwayLength),
            wallMaterial, true);
    }

    // ── 3 Side rooms — 2 on the left, 1 on the right ─────────────────────
    void BuildRooms()
    {
        // Left side rooms (at X = -hallwayWidth/2 - roomWidth/2)
        BuildRoom("Room1", -1, 2f,                                 true,  Color.blue);
        BuildRoom("Room2", -1, 2f + roomDepth + 2f,               true,  Color.yellow);

        // Right side room
        BuildRoom("Room3",  1, 2f,                                 false, Color.red);
    }

    // side: -1 = left, 1 = right
    void BuildRoom(string name, int side, float zStart, bool isLeft, Color breakerHint)
    {
        GameObject room = new GameObject(name);
        room.transform.SetParent(_root.transform, false);

        float halfHall = hallwayWidth * 0.5f;
        float mid      = wallHeight * 0.5f;
        float aboveH   = wallHeight - doorHeight;

        // Room extends outward from the hallway wall
        float wallX    = side * (halfHall + roomWidth * 0.5f);
        float doorX    = side * halfHall;
        float outerX   = side * (halfHall + roomWidth);
        float zMid     = zStart + roomDepth * 0.5f;
        float zEnd     = zStart + roomDepth;

        // Floor
        Slab(room, $"{name}_Floor",
            new Vector3(wallX, -T * 0.5f, zMid),
            new Vector3(roomWidth, T, roomDepth),
            floorMaterial, true);

        // Ceiling
        Slab(room, $"{name}_Ceiling",
            new Vector3(wallX, wallHeight + T * 0.5f, zMid),
            new Vector3(roomWidth, T, roomDepth),
            ceilingMaterial, true);

        // Outer wall (far from hallway)
        Slab(room, $"{name}_WallOuter",
            new Vector3(outerX + side * T * 0.5f, mid, zMid),
            new Vector3(T, wallHeight, roomDepth),
            wallMaterial, true);

        // Front wall (near Z start)
        Slab(room, $"{name}_WallFront",
            new Vector3(wallX, mid, zStart - T * 0.5f),
            new Vector3(roomWidth, wallHeight, T),
            wallMaterial, true);

        // Back wall (near Z end)
        Slab(room, $"{name}_WallBack",
            new Vector3(wallX, mid, zEnd + T * 0.5f),
            new Vector3(roomWidth, wallHeight, T),
            wallMaterial, true);

        // Hallway-facing wall with door opening
        // Left section of hallway wall (before door)
        float doorZStart = zStart + (roomDepth - doorWidth) * 0.5f;
        float doorZEnd   = doorZStart + doorWidth;

        float seg1Len    = doorZStart - zStart;
        float seg2Len    = zEnd - doorZEnd;

        if (seg1Len > 0.01f)
            Slab(room, $"{name}_HallWall_Before",
                new Vector3(doorX + side * T * 0.5f, mid, zStart + seg1Len * 0.5f),
                new Vector3(T, wallHeight, seg1Len),
                wallMaterial, true);

        if (seg2Len > 0.01f)
            Slab(room, $"{name}_HallWall_After",
                new Vector3(doorX + side * T * 0.5f, mid, doorZEnd + seg2Len * 0.5f),
                new Vector3(T, wallHeight, seg2Len),
                wallMaterial, true);

        // Door header above opening
        if (aboveH > 0.01f)
            Slab(room, $"{name}_DoorHeader",
                new Vector3(doorX + side * T * 0.5f, doorHeight + aboveH * 0.5f, doorZStart + doorWidth * 0.5f),
                new Vector3(T, aboveH, doorWidth),
                wallMaterial, true);

        // Wooden door in the opening
        BuildWoodenDoor(room, $"{name}_Door",
            new Vector3(doorX + side * T * 0.5f, doorHeight * 0.5f, doorZStart + doorWidth * 0.5f),
            true);

        // Room ceiling light — starts OFF (breaker controls it)
        BuildLEDPanel(room, new Vector3(wallX, wallHeight, zMid), 2f, 0.5f, false);

        // Small breaker placement hint marker (just a coloured slab on the back wall)
        Slab(room, $"{name}_BreakerMount",
            new Vector3(wallX + side * roomWidth * 0.3f, 2f, zEnd - 0.05f),
            new Vector3(0.45f, 0.65f, 0.1f),
            wallMaterial, false);

        Debug.Log($"[Floor5Builder] {name} built. Place breaker on BreakerMount at Z={zEnd}");
    }

    // ── Server room — at the far end of the left side ─────────────────────
    void BuildServerRoom()
    {
        GameObject room = new GameObject("ServerRoom");
        room.transform.SetParent(_root.transform, false);

        float halfHall = hallwayWidth * 0.5f;
        float mid      = wallHeight * 0.5f;
        float aboveH   = wallHeight - doorHeight;

        float zStart   = hallwayLength - serverRoomDepth - 2f;
        float zEnd     = hallwayLength - 2f;
        float zMid     = (zStart + zEnd) * 0.5f;
        float outerX   = -halfHall - serverRoomWidth;
        float wallX    = -halfHall - serverRoomWidth * 0.5f;
        float doorX    = -halfHall;

        // Floor
        Slab(room, "Server_Floor",
            new Vector3(wallX, -T * 0.5f, zMid),
            new Vector3(serverRoomWidth, T, serverRoomDepth),
            floorMaterial, true);

        // Ceiling
        Slab(room, "Server_Ceiling",
            new Vector3(wallX, wallHeight + T * 0.5f, zMid),
            new Vector3(serverRoomWidth, T, serverRoomDepth),
            ceilingMaterial, true);

        // Outer wall
        Slab(room, "Server_WallOuter",
            new Vector3(outerX - T * 0.5f, mid, zMid),
            new Vector3(T, wallHeight, serverRoomDepth),
            wallMaterial, true);

        // Front wall
        Slab(room, "Server_WallFront",
            new Vector3(wallX, mid, zStart - T * 0.5f),
            new Vector3(serverRoomWidth, wallHeight, T),
            wallMaterial, true);

        // Back wall
        Slab(room, "Server_WallBack",
            new Vector3(wallX, mid, zEnd + T * 0.5f),
            new Vector3(serverRoomWidth, wallHeight, T),
            wallMaterial, true);

        // Hallway wall with door
        float doorZStart = zStart + (serverRoomDepth - doorWidth) * 0.5f;
        float doorZEnd   = doorZStart + doorWidth;
        float seg1Len    = doorZStart - zStart;
        float seg2Len    = zEnd - doorZEnd;

        if (seg1Len > 0.01f)
            Slab(room, "Server_HallWall_Before",
                new Vector3(doorX - T * 0.5f, mid, zStart + seg1Len * 0.5f),
                new Vector3(T, wallHeight, seg1Len),
                wallMaterial, true);

        if (seg2Len > 0.01f)
            Slab(room, "Server_HallWall_After",
                new Vector3(doorX - T * 0.5f, mid, doorZEnd + seg2Len * 0.5f),
                new Vector3(T, wallHeight, seg2Len),
                wallMaterial, true);

        if (aboveH > 0.01f)
            Slab(room, "Server_DoorHeader",
                new Vector3(doorX - T * 0.5f, doorHeight + aboveH * 0.5f, doorZStart + doorWidth * 0.5f),
                new Vector3(T, aboveH, doorWidth),
                wallMaterial, true);

        BuildWoodenDoor(room, "Server_Door",
            new Vector3(-2.831f, 2f, 44.256f),
            false);

        // Server room gets a green glow monitor on the back wall
        // (used as hallway monitor feedback for B3)
        Slab(room, "Monitor_B3",
            new Vector3(wallX, wallHeight * 0.5f, zEnd - 0.06f),
            new Vector3(1.6f, 1.0f, 0.06f),
            glassMaterial != null ? glassMaterial : wallMaterial, false);

        // Server room breaker mount
        Slab(room, "Server_BreakerMount",
            new Vector3(outerX + 0.08f, 2f, zMid),
            new Vector3(0.1f, 0.65f, 0.45f),
            wallMaterial, false);

        Debug.Log("[Floor5Builder] ServerRoom built. B5 breaker goes on Server_BreakerMount.");
    }

    // ── Corridor ceiling lights ────────────────────────────────────────────
    void BuildLights()
    {
        GameObject lights = new GameObject("HallwayLights");
        lights.transform.SetParent(_root.transform, false);

        float spacing = 8f;
        int   count   = Mathf.Max(1, Mathf.FloorToInt(hallwayLength / spacing));
        for (int i = 0; i < count; i++)
        {
            float z = spacing * i + spacing * 0.5f;
            BuildLEDPanel(lights, new Vector3(0f, wallHeight, z), 1.2f, 0.4f, true);
        }
    }

    // ── LED Panel helper ───────────────────────────────────────────────────
    void BuildLEDPanel(GameObject parent, Vector3 pos,
                       float panelX, float panelZ, bool startsOn)
    {
        const float ft = 0.03f;
        const float pd = 0.03f;

        GameObject fix = new GameObject("LEDPanel");
        fix.transform.SetParent(parent.transform, false);
        fix.transform.localPosition = new Vector3(pos.x, pos.y - T * 0.5f, pos.z);

        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "Panel_Frame";
        frame.transform.SetParent(fix.transform, false);
        frame.transform.localPosition = new Vector3(0f, -ft * 0.5f, 0f);
        frame.transform.localScale    = new Vector3(panelX, ft, panelZ);
        ApplyMat(frame, panelFrameMaterial != null ? panelFrameMaterial : wallMaterial);
        SetStatic(frame);

        GameObject diff = GameObject.CreatePrimitive(PrimitiveType.Cube);
        diff.name = "Panel_Diffuser";
        diff.transform.SetParent(fix.transform, false);
        diff.transform.localPosition = new Vector3(0f, -ft - pd * 0.5f, 0f);
        diff.transform.localScale    = new Vector3(panelX - ft * 3f, pd, panelZ - ft * 3f);
        ApplyMat(diff, panelDiffuserMaterial != null ? panelDiffuserMaterial : wallMaterial);
        SetStatic(diff);

        GameObject lightGO = new GameObject("PointLight");
        lightGO.transform.SetParent(fix.transform, false);
        lightGO.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        Light lt     = lightGO.AddComponent<Light>();
        lt.type      = LightType.Point;
        lt.range     = lightRange;
        lt.intensity = lightIntensity;
        lt.color     = normalLightColor;
        lt.shadows   = LightShadows.None;
        lt.enabled   = startsOn;

        if (startsOn) lightGO.AddComponent<LightFlicker>();
    }

    // ── Wooden door ────────────────────────────────────────────────────────
    void BuildWoodenDoor(GameObject parent, string name, Vector3 pos, bool facingX)
    {
        GameObject door = new GameObject(name);
        door.transform.SetParent(parent.transform, false);
        door.transform.localPosition = pos;

        Material mat = doorMaterial != null ? doorMaterial : wallMaterial;

        // Main panel
        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "DoorPanel";
        panel.transform.SetParent(door.transform, false);
        panel.transform.localPosition = Vector3.zero;
        panel.transform.localScale    = new Vector3(
            facingX ? 0.08f : doorWidth,
            doorHeight,
            facingX ? doorWidth : 0.08f);
        ApplyMat(panel, mat);

        // Glass window in upper portion
        GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
        window.name = "DoorWindow";
        window.transform.SetParent(door.transform, false);
        window.transform.localPosition = new Vector3(
            facingX ? 0.05f : 0f,
            doorHeight * 0.2f,
            facingX ? 0f : 0.05f);
        window.transform.localScale    = new Vector3(
            facingX ? 0.06f : doorWidth * 0.4f,
            doorHeight * 0.3f,
            facingX ? doorWidth * 0.4f : 0.06f);
        ApplyMat(window, glassMaterial != null ? glassMaterial : wallMaterial);

        // Frame top
        GameObject frameTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameTop.name = "Frame_Top";
        frameTop.transform.SetParent(door.transform, false);
        frameTop.transform.localPosition = new Vector3(0f, doorHeight * 0.5f + 0.04f, 0f);
        frameTop.transform.localScale    = new Vector3(
            facingX ? 0.1f : doorWidth + 0.1f,
            0.08f,
            facingX ? doorWidth + 0.1f : 0.1f);
        ApplyMat(frameTop, wallMaterial);
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    GameObject Slab(GameObject parent, string objName,
                    Vector3 pos, Vector3 scale, Material mat, bool isStatic)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = scale;
        ApplyMat(go, mat ?? wallMaterial);
        if (isStatic) SetStatic(go);
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