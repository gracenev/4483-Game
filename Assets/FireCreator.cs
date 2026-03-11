using UnityEngine;

public class FireCreator : MonoBehaviour
{
    public Material fireMaterial;
    public Material smokeMaterial;
    public Material sparksMaterial;
    public float fireScale = 1f;

    void Start()
    {
        CreateFire();
        CreateSmoke();
        CreateSparks();
    }

    void CreateFire()
    {
        GameObject fireObj = new GameObject("Fire");
        fireObj.transform.SetParent(transform);
        fireObj.transform.localPosition = Vector3.zero;

        ParticleSystem ps = fireObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 1.2f;
        main.startSpeed = 1.5f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f * fireScale, 1.5f * fireScale);
        main.startColor = new Color(1f, 0.6f, 0.1f, 1f);
        main.gravityModifier = -0.4f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 200;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 40;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f * fireScale;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.8f, 0.2f), 0f),
                new GradientColorKey(new Color(1f, 0.4f, 0f), 0.4f),
                new GradientColorKey(new Color(0.8f, 0.1f, 0f), 0.8f),
                new GradientColorKey(new Color(0.2f, 0f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.1f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        noise.frequency = 1.5f;
        noise.scrollSpeed = 0.5f;

        var textureSheet = ps.textureSheetAnimation;
        textureSheet.enabled = true;
        textureSheet.numTilesX = 2;
        textureSheet.numTilesY = 2;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = fireMaterial;

        ps.Play();
    }

    void CreateSmoke()
    {
        GameObject smokeObj = new GameObject("Smoke");
        smokeObj.transform.SetParent(transform);
        smokeObj.transform.localPosition = Vector3.zero;

        ParticleSystem ps = smokeObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 3f;
        main.startSpeed = 0.8f;
        main.startSize = new ParticleSystem.MinMaxCurve(1f * fireScale, 2f * fireScale);
        main.startColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        main.gravityModifier = -0.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 100;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 15;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f * fireScale;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.3f, 0.3f, 0.3f), 0f),
                new GradientColorKey(new Color(0.2f, 0.2f, 0.2f), 0.5f),
                new GradientColorKey(new Color(0.1f, 0.1f, 0.1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.3f, 0.2f),
                new GradientAlphaKey(0.2f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(1f, 1.5f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.8f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = smokeMaterial;

        ps.Play();
    }

    void CreateSparks()
    {
        GameObject sparksObj = new GameObject("Sparks");
        sparksObj.transform.SetParent(transform);
        sparksObj.transform.localPosition = Vector3.zero;

        ParticleSystem ps = sparksObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f * fireScale, 0.15f * fireScale);
        main.startColor = new Color(1f, 0.7f, 0.2f, 1f);
        main.gravityModifier = -0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 50;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 10;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 5, 10, 1, 0.5f)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.1f * fireScale;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0f),
                new GradientColorKey(new Color(1f, 0.4f, 0f), 0.5f),
                new GradientColorKey(new Color(0.5f, 0.1f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 1f;
        noise.frequency = 3f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = sparksMaterial;

        ps.Play();
    }
}