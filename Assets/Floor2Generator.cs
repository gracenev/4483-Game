using UnityEngine;
using System.Collections.Generic;
 
/// <summary>
/// Generates Floor 2 — Surveillance Constraint.
/// Long hallway, rooms on both sides, closed glass sliding doors,
/// LED panel ceiling lights with flickering.
///
/// SETUP:
///   1. Create an empty GameObject, attach this script.
///   2. Assign materials in the Inspector.
///   3. Right-click → "Generate Floor 2".
///
/// Wall height is 6 to match the building spec.
/// </summary>
public class Floor2Generator : MonoBehaviour
{
    [Header("Hallway")]
    public float hallwayWidth  = 5f;
    public float hallwayHeight = 6f;
    public float hallwayLength = 60f;
 
    [Header("Rooms")]
    public int   roomCountPerSide = 4;
    public float roomWidth        = 7f;
    public float roomDepth        = 9f;
 
    [Header("Doors")]
    public float doorWidth  = 2.5f;
    public float doorHeight = 3f;    // tall doors for 6m ceiling
 
    [Header("Lighting")]
    public float flickerChance  = 0.3f;
    public float lightRange     = 10f;
    public float lightIntensity = 3f;
    public Color lightColor     = new Color(1f, 0.95f, 0.85f);
 
    [Header("Materials")]
    public Material wallMaterial;
    public Material floorMaterial;
    public Material ceilingMaterial;
    public Material glassMaterial;
    public Material panelFrameMaterial;
    public Material panelDiffuserMaterial;
 
    private const float T = 0.2f;   // slab thickness
    private GameObject _root;
 
    // ── Context menus ──────────────────────────────────────────────────────
    [ContextMenu("Generate Floor 2")]
    public void Generate()
    {
        Clear();
        _root = new GameObject("Floor2_Root");
        _root.transform.SetParent(transform, false);
 
        BuildHallway();
        BuildRooms();
        BuildHallwayGapWalls();
    }
 
    [ContextMenu("Clear Floor 2")]
    public void Clear()
    {
        Transform existing = transform.Find("Floor2_Root");
        if (existing != null) DestroyImmediate(existing.gameObject);
        if (_root    != null) DestroyImmediate(_root);
    }
 
    // ── Hallway ────────────────────────────────────────────────────────────
    void BuildHallway()
    {
        GameObject hall = new GameObject("Hallway");
        hall.transform.SetParent(_root.transform, false);
 
        float cx = 0f;
        float cz = hallwayLength * 0.5f;
 
        // Floor
        Slab(hall, "Hall_Floor",
            new Vector3(cx, 0f, cz),
            new Vector3(hallwayWidth, T, hallwayLength),
            floorMaterial);
 
        // Ceiling
        Slab(hall, "Hall_Ceiling",
            new Vector3(cx, hallwayHeight, cz),
            new Vector3(hallwayWidth, T, hallwayLength),
            ceilingMaterial);
 
        // Back wall (entrance end Z=0)
        Slab(hall, "Hall_WallBack",
            new Vector3(cx, hallwayHeight * 0.5f, 0f),
            new Vector3(hallwayWidth, hallwayHeight, T),
            wallMaterial);
 
        // Far end wall
        Slab(hall, "Hall_WallFar",
            new Vector3(cx, hallwayHeight * 0.5f, hallwayLength),
            new Vector3(hallwayWidth, hallwayHeight, T),
            wallMaterial);
 
        // LED panels along hallway ceiling — one every 6m
        float spacing = 6f;
        int   count   = Mathf.Max(1, Mathf.FloorToInt(hallwayLength / spacing));
        for (int i = 0; i < count; i++)
        {
            float lz      = spacing * i + spacing * 0.5f;
            bool  flicker = (i % 3 == 2);   // every 3rd panel flickers
            BuildLEDPanel(hall,
                new Vector3(cx, hallwayHeight, lz),
                hallwayWidth * 0.65f, 0.6f, flicker);
        }
    }
 
    // ── Rooms ──────────────────────────────────────────────────────────────
    void BuildRooms()
    {
        float spacing = hallwayLength / roomCountPerSide;
 
        for (int i = 0; i < roomCountPerSide; i++)
        {
            float zc = spacing * i + spacing * 0.5f;
            BuildRoom(i, zc, -1);   // left
            BuildRoom(i, zc,  1);   // right
        }
    }
 
    void BuildRoom(int index, float zCenter, int side)
    {
        string name = $"Room_{(side < 0 ? "Left" : "Right")}_{index + 1}";
        GameObject room = new GameObject(name);
        room.transform.SetParent(_root.transform, false);
 
        float edgeX   = side * hallwayWidth * 0.5f;
        float outerX  = edgeX + side * roomWidth;
        float centreX = (edgeX + outerX) * 0.5f;
        float zMin    = zCenter - roomDepth * 0.5f;
        float zMax    = zCenter + roomDepth * 0.5f;
        float mid     = hallwayHeight * 0.5f;
 
        // Floor
        Slab(room, "Floor",
            new Vector3(centreX, 0f, zCenter),
            new Vector3(roomWidth, T, roomDepth), floorMaterial);
 
        // Ceiling
        Slab(room, "Ceiling",
            new Vector3(centreX, hallwayHeight, zCenter),
            new Vector3(roomWidth, T, roomDepth), ceilingMaterial);
 
        // Outer wall
        Slab(room, "WallOuter",
            new Vector3(outerX, mid, zCenter),
            new Vector3(T, hallwayHeight, roomDepth), wallMaterial);
 
        // Front wall
        Slab(room, "WallFront",
            new Vector3(centreX, mid, zMin),
            new Vector3(roomWidth, hallwayHeight, T), wallMaterial);
 
        // Back wall
        Slab(room, "WallBack",
            new Vector3(centreX, mid, zMax),
            new Vector3(roomWidth, hallwayHeight, T), wallMaterial);
 
        // Inner wall (hallway-facing) with door cutout
        float innerX  = edgeX;
        float sideLen = (roomDepth - doorWidth) * 0.5f;
        float aboveH  = hallwayHeight - doorHeight;
 
        // Above door
        if (aboveH > 0.01f)
            Slab(room, "WallInner_Above",
                new Vector3(innerX, doorHeight + aboveH * 0.5f, zCenter),
                new Vector3(T, aboveH, roomDepth), wallMaterial);
 
        // Left strip
        if (sideLen > 0.01f)
        {
            Slab(room, "WallInner_Left",
                new Vector3(innerX, mid, zMin + sideLen * 0.5f),
                new Vector3(T, hallwayHeight, sideLen), wallMaterial);
 
            Slab(room, "WallInner_Right",
                new Vector3(innerX, mid, zMax - sideLen * 0.5f),
                new Vector3(T, hallwayHeight, sideLen), wallMaterial);
        }
 
        // Closed glass sliding door
        BuildSlidingDoor(room, innerX, zCenter);
 
        // LED panel light in room ceiling
        bool flicker = (index % Mathf.RoundToInt(1f / Mathf.Max(0.01f, flickerChance))) == 0;
        BuildLEDPanel(room,
            new Vector3(centreX, hallwayHeight, zCenter),
            roomWidth * 0.5f, 0.6f, flicker);
    }
 
    // ── Closed sliding glass door ──────────────────────────────────────────
    void BuildSlidingDoor(GameObject parent, float wallX, float zCenter)
    {
        const float glassThick = 0.05f;
        const float frameW     = 0.06f;
 
        GameObject door = new GameObject("Door");
        door.transform.SetParent(parent.transform, false);
 
        // Track
        Slab(door, "Track",
            new Vector3(wallX, doorHeight + frameW * 0.5f, zCenter),
            new Vector3(glassThick + frameW * 2f, frameW, doorWidth + frameW * 2f),
            wallMaterial);
 
        // Left jamb
        Slab(door, "JambLeft",
            new Vector3(wallX, doorHeight * 0.5f, zCenter - doorWidth * 0.5f - frameW * 0.5f),
            new Vector3(glassThick + frameW, doorHeight, frameW),
            wallMaterial);
 
        // Right jamb
        Slab(door, "JambRight",
            new Vector3(wallX, doorHeight * 0.5f, zCenter + doorWidth * 0.5f + frameW * 0.5f),
            new Vector3(glassThick + frameW, doorHeight, frameW),
            wallMaterial);
 
        // Glass panel — CLOSED (centred in opening, not slid open)
        GameObject panel = new GameObject("Door_Panel");
        panel.transform.SetParent(door.transform, false);
        panel.transform.localPosition = new Vector3(wallX, 0f, zCenter);
 
        // Glass fill
        Slab(panel, "Glass",
            new Vector3(0f, doorHeight * 0.5f, 0f),
            new Vector3(glassThick, doorHeight - frameW, doorWidth - frameW),
            glassMaterial != null ? glassMaterial : wallMaterial);
 
        // Panel edge frames
        Slab(panel, "Frame_Top",
            new Vector3(0f, doorHeight - frameW * 0.5f, 0f),
            new Vector3(glassThick + 0.02f, frameW, doorWidth), wallMaterial);
        Slab(panel, "Frame_Bottom",
            new Vector3(0f, frameW * 0.5f, 0f),
            new Vector3(glassThick + 0.02f, frameW, doorWidth), wallMaterial);
        Slab(panel, "Frame_Lead",
            new Vector3(0f, doorHeight * 0.5f, -doorWidth * 0.5f),
            new Vector3(glassThick + 0.02f, doorHeight, frameW), wallMaterial);
        Slab(panel, "Frame_Trail",
            new Vector3(0f, doorHeight * 0.5f, doorWidth * 0.5f),
            new Vector3(glassThick + 0.02f, doorHeight, frameW), wallMaterial);
 
        // Pull bars
        const float barLen   = 0.4f;
        const float barThick = 0.03f;
        float barZ = -doorWidth * 0.5f + frameW + 0.08f;
 
        Slab(panel, "Handle_Front",
            new Vector3( glassThick * 0.5f + barThick * 0.5f + 0.01f, doorHeight * 0.5f, barZ),
            new Vector3(barThick, barLen, barThick), wallMaterial);
        Slab(panel, "Handle_Back",
            new Vector3(-glassThick * 0.5f - barThick * 0.5f - 0.01f, doorHeight * 0.5f, barZ),
            new Vector3(barThick, barLen, barThick), wallMaterial);
    }
 
    // ── Gap walls ──────────────────────────────────────────────────────────
    void BuildHallwayGapWalls()
    {
        float spacing    = hallwayLength / roomCountPerSide;
        float leftEdgeX  = -hallwayWidth * 0.5f;
        float rightEdgeX =  hallwayWidth * 0.5f;
        float mid        = hallwayHeight * 0.5f;
 
        var starts = new List<float>();
        var ends   = new List<float>();
        float cursor = 0f;
 
        for (int i = 0; i < roomCountPerSide; i++)
        {
            float zc   = spacing * i + spacing * 0.5f;
            float zMin = zc - roomDepth * 0.5f;
            float zMax = zc + roomDepth * 0.5f;
 
            if (zMin - cursor > 0.01f) { starts.Add(cursor); ends.Add(zMin); }
            cursor = zMax;
        }
        if (hallwayLength - cursor > 0.01f) { starts.Add(cursor); ends.Add(hallwayLength); }
 
        GameObject gapRoot = new GameObject("GapWalls");
        gapRoot.transform.SetParent(_root.transform, false);
 
        for (int g = 0; g < starts.Count; g++)
        {
            float zc  = (starts[g] + ends[g]) * 0.5f;
            float len = ends[g] - starts[g];
 
            Slab(gapRoot, $"Gap_L_{g}",
                new Vector3(leftEdgeX,  mid, zc),
                new Vector3(T, hallwayHeight, len), wallMaterial);
 
            Slab(gapRoot, $"Gap_R_{g}",
                new Vector3(rightEdgeX, mid, zc),
                new Vector3(T, hallwayHeight, len), wallMaterial);
        }
    }
 
    // ── LED Panel ──────────────────────────────────────────────────────────
    void BuildLEDPanel(GameObject parent, Vector3 ceilingPos,
                       float panelX, float panelZ, bool flicker)
    {
        const float frameThick = 0.03f;
        const float panelDepth = 0.03f;
 
        float bottomY = ceilingPos.y - T * 0.5f;
 
        GameObject fixture = new GameObject("LEDPanel");
        fixture.transform.SetParent(parent.transform, false);
        fixture.transform.localPosition = new Vector3(ceilingPos.x, bottomY, ceilingPos.z);
 
        // Frame
        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "Panel_Frame";
        frame.transform.SetParent(fixture.transform, false);
        frame.transform.localPosition = new Vector3(0f, -frameThick * 0.5f, 0f);
        frame.transform.localScale    = new Vector3(panelX, frameThick, panelZ);
        ApplyMat(frame, panelFrameMaterial != null ? panelFrameMaterial : wallMaterial);
        SetStatic(frame);
 
        // Diffuser
        GameObject diff = GameObject.CreatePrimitive(PrimitiveType.Cube);
        diff.name = "Panel_Diffuser";
        diff.transform.SetParent(fixture.transform, false);
        diff.transform.localPosition = new Vector3(0f, -frameThick - panelDepth * 0.5f, 0f);
        diff.transform.localScale    = new Vector3(panelX - frameThick * 3f,
                                                    panelDepth,
                                                    panelZ - frameThick * 3f);
        ApplyMat(diff, panelDiffuserMaterial != null ? panelDiffuserMaterial : wallMaterial);
        SetStatic(diff);
 
        // Point light
        GameObject lightGO = new GameObject("PointLight");
        lightGO.transform.SetParent(fixture.transform, false);
        lightGO.transform.localPosition = new Vector3(0f, -0.5f, 0f);
 
        Light lt       = lightGO.AddComponent<Light>();
        lt.type        = LightType.Point;
        lt.range       = lightRange;
        lt.intensity   = lightIntensity;
        lt.color       = lightColor;
        lt.shadows     = LightShadows.Soft;
 
        if (flicker)
            lightGO.AddComponent<LightFlicker>();
    }
 
    // ── Helpers ────────────────────────────────────────────────────────────
    GameObject Slab(GameObject parent, string objName,
                    Vector3 pos, Vector3 scale, Material mat = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = pos;
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