using UnityEngine;
using System.Collections.Generic;

// Watches the camera state and shifts all the hallway lights to red
// when the cameras are facing the player, then back to normal when they look away.
public class RedLightEffect : MonoBehaviour
{
    [Header("Colors")]
    public Color normalColor   = new Color(1f, 0.95f, 0.85f);  // warm white during green light
    public Color redLightColor = new Color(1f, 0.05f, 0.05f);  // red during camera watch

    [Header("Transition")]
    [Tooltip("How fast the lights shift between normal and red")]
    public float transitionSpeed = 4f;

    private CameraSync _sync;
    private Light[]    _lights;

    void Start()
    {
        _sync = FindAnyObjectByType<CameraSync>();
        if (_sync == null)
            Debug.LogWarning("RedLightEffect couldn't find CameraSync in the scene.");

        // Grab every Point Light in the scene - these are the LED ceiling panels
        var lights = new List<Light>();
        foreach (Light lt in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (lt.type == LightType.Point)
                lights.Add(lt);
        _lights = lights.ToArray();

        Debug.Log($"RedLightEffect found {_lights.Length} lights to control.");
    }

    void Update()
    {
        if (_sync == null) return;

        // Turn red as soon as the cameras start turning - not just when fully facing
        bool isRed = _sync.State == CameraSync.CamState.FacingPlayer ||
                     _sync.State == CameraSync.CamState.TurningToPlayer;

        Color target = isRed ? redLightColor : normalColor;

        // Smoothly lerp each light toward the target colour each frame
        foreach (var lt in _lights)
            lt.color = Color.Lerp(lt.color, target, transitionSpeed * Time.deltaTime);
    }
}