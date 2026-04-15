using UnityEngine;

// Spawns a physical breaker box in the scene.
// Attach to an empty GameObject, position it on a wall, then
// right-click -> "Build Breaker Box".
// After spawning, assign the resulting BreakerSwitch component
// to the Floor5LevelManager's breakers array.
public class BreakerBoxBuilder : MonoBehaviour
{
    [Header("Breaker Identity")]
    public int    breakerID    = 0;
    public string breakerLabel = "B1";
    public Color  breakerColor = Color.blue;

    [Header("Materials")]
    public Material boxMaterial;
    public Material leverMaterial;
    public Material labelMaterial;

    [ContextMenu("Build Breaker Box")]
    public void Build()
    {
        // Clear previous
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        // Box body
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = $"BreakerBox_{breakerLabel}";
        box.transform.SetParent(transform, false);
        box.transform.localPosition = Vector3.zero;
        box.transform.localScale    = new Vector3(0.4f, 0.6f, 0.08f);
        ApplyMat(box, boxMaterial, new Color(0.12f, 0.12f, 0.12f));

        // Lever handle
        GameObject lever = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lever.name = "Lever";
        lever.transform.SetParent(box.transform, false);
        lever.transform.localPosition = new Vector3(0f, 0.15f, -0.55f);
        lever.transform.localScale    = new Vector3(0.3f, 0.12f, 0.4f);
        ApplyMat(lever, leverMaterial, new Color(0.8f, 0.8f, 0.8f));

        // Coloured status light (small square on the front)
        GameObject light = GameObject.CreatePrimitive(PrimitiveType.Cube);
        light.name = "StatusLight";
        light.transform.SetParent(box.transform, false);
        light.transform.localPosition = new Vector3(0f, -0.2f, -0.55f);
        light.transform.localScale    = new Vector3(0.15f, 0.15f, 0.1f);
        ApplyMat(light, null, breakerColor);

        // Add the BreakerSwitch component to the box
        BreakerSwitch sw     = box.AddComponent<BreakerSwitch>();
        sw.breakerID         = breakerID;
        sw.breakerName       = breakerLabel;
        sw.breakerColor      = breakerColor;
        sw.leverTransform    = lever.transform;

        Debug.Log($"[BreakerBoxBuilder] Built {breakerLabel} — assign to Floor5LevelManager.breakers[{breakerID}]");
    }

    void ApplyMat(GameObject go, Material mat, Color fallback)
    {
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null) return;
        if (mat != null)
            mr.sharedMaterial = mat;
        else
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color = fallback;
            mr.material = m;
        }
    }
}