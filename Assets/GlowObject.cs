using UnityEngine;

public class GlowObject : MonoBehaviour
{
    public Color glowColor = new Color(0f, 0f, 0f);
    public float glowIntensity = 2f;
    public float lightRange = 5f;
    public float flickerSpeed = 3f;
    public float flickerAmount = 0.3f;
    public bool flicker = true;

    private Light pointLight;
    private Material mat;
    private float baseIntensity;

    void Start()
    {
        // Add a point light
        GameObject lightObj = new GameObject("Glow Light");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = glowColor;
        pointLight.intensity = glowIntensity;
        pointLight.range = lightRange;
        baseIntensity = glowIntensity;

        // Make the object itself glow
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
        if (flicker)
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            float intensity = baseIntensity + (noise - 0.5f) * flickerAmount * baseIntensity;
            pointLight.intensity = intensity;

            if (mat != null)
            {
                mat.SetColor("_EmissionColor", glowColor * intensity);
            }
        }
    }
}