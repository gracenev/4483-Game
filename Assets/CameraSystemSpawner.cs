using UnityEngine;

/// <summary>
/// Spawns 4 surveillance cameras on the hallway ceiling.
/// Each camera runs itself via SurveillanceCamera — delete this
/// script after spawning and everything keeps working.
/// </summary>
public class CameraSystemSpawner : MonoBehaviour
{
    [Header("Hallway Reference")]
    public float hallwayLength = 160f;
    public float hallwayHeight = 6f;
    public float hallwayWidth  = 10f;

    [Header("Camera Placement")]
    public int   cameraCount = 4;
    public float hangOffset  = 0.3f;

    [Header("Materials")]
    public Material bodyMaterial;
    public Material lensMaterial;

    [ContextMenu("Spawn Cameras Now")]
    public void SpawnCameras()
    {
        // Clear old cameras
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        float startZ  = hallwayLength * 0.15f;
        float endZ    = hallwayLength * 0.85f;
        float spacing = (endZ - startZ) / (cameraCount - 1);

        for (int i = 0; i < cameraCount; i++)
        {
            float z   = startZ + spacing * i;
            GameObject cam = BuildCameraModel(i, z);
            // SurveillanceCamera drives itself — no wiring needed
            cam.AddComponent<SurveillanceCamera>();
        }
    }

    GameObject BuildCameraModel(int index, float z)
    {
        GameObject root = new GameObject($"SurveillanceCam_{index + 1}");
        root.transform.SetParent(transform, false);
        root.transform.position = new Vector3(
            transform.position.x,
            transform.position.y + hallwayHeight - hangOffset,
            transform.position.z + z);

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "CamBody";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale    = new Vector3(0.8f, 0.5f, 1.1f);
        ApplyMat(body, bodyMaterial, new Color(0.15f, 0.15f, 0.15f));

        // Mount bracket
        GameObject mount = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mount.name = "CamMount";
        mount.transform.SetParent(root.transform, false);
        mount.transform.localPosition = new Vector3(0f, hangOffset * 0.5f, 0f);
        mount.transform.localScale    = new Vector3(0.18f, hangOffset, 0.18f);
        ApplyMat(mount, bodyMaterial, new Color(0.15f, 0.15f, 0.15f));

        // Lens
        GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lens.name = "CamLens";
        lens.transform.SetParent(root.transform, false);
        lens.transform.localPosition = new Vector3(0f, -0.1f, 0.6f);
        lens.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        lens.transform.localScale    = new Vector3(0.25f, 0.25f, 0.25f);
        ApplyMat(lens, lensMaterial, new Color(0.8f, 0.1f, 0.1f));

        return root;
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