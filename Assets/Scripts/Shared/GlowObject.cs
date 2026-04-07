using UnityEngine;

// Makes an object glow by adding a point light and enabling emission on its material.
// Can also flicker using Perlin noise to give it a more organic feel.
public class GlowObject : MonoBehaviour
{
    public Color glowColor     = new Color(0f, 0f, 0f);
    public float glowIntensity = 2f;
    public float lightRange    = 5f;
    public float flickerSpeed  = 3f;
    public float flickerAmount = 0.3f;
    public bool  flicker       = true;

    private Light    pointLight;
    private Material mat;
    private float    baseIntensity;

    void Start()
    {
        // Create a child point light to cast the glow into the scene
        GameObject lightObj = new GameObject("Glow Light");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;

        pointLight           = lightObj.AddComponent<Light>();
        pointLight.type      = LightType.Point;
        pointLight.color     = glowColor;
        pointLight.intensity = glowIntensity;
        pointLight.range     = lightRange;
        baseIntensity        = glowIntensity;

        // Also make the object's own surface appear to emit light
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", glowColor * glowIntensity);
        }
    }

    void Update()
    {
        if (!flicker) return;

        // Use Perlin noise to gently vary the intensity each frame
        float noise     = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        float intensity = baseIntensity + (noise - 0.5f) * flickerAmount * baseIntensity;

        pointLight.intensity = intensity;

        if (mat != null)
            mat.SetColor("_EmissionColor", glowColor * intensity);
    }
}