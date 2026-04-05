using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Watches all SurveillanceCamera states and transitions the hallway
/// lights to red when any camera is facing the player, back to normal
/// when all cameras are looking away.
///
/// SETUP:
///   1. Attach to any persistent GameObject in the Floor 2 scene.
///   2. Set normalColor to match your LED panel light color.
///   3. Hit Play — no other setup needed, finds lights automatically.
/// </summary>
public class RedLightEffect : MonoBehaviour
{
    [Header("Light Colors")]
    public Color normalColor    = new Color(1f, 0.95f, 0.85f);   // warm white
    public Color redLightColor  = new Color(1f, 0.05f, 0.05f);   // red

    [Header("Transition")]
    [Tooltip("How fast the lights blend between normal and red")]
    public float transitionSpeed = 4f;

    // ── Runtime ────────────────────────────────────────────────────────────
    private SurveillanceCamera[] _cameras;
    private Light[]              _lights;
    private Color                _targetColor;

    void Start()
    {
        // Find all surveillance cameras in the scene
        _cameras = FindObjectsOfType<SurveillanceCamera>();
        if (_cameras.Length == 0)
            Debug.LogWarning("[RedLightEffect] No SurveillanceCameras found!");

        // Find all Point Lights that are children of LEDPanel objects
        // (named "PointLight" by Floor2Generator)
        var lights = new List<Light>();
        foreach (Light lt in FindObjectsOfType<Light>())
        {
            if (lt.type == LightType.Point)
                lights.Add(lt);
        }
        _lights = lights.ToArray();
        Debug.Log($"[RedLightEffect] Tracking {_lights.Length} lights, {_cameras.Length} cameras.");

        _targetColor = normalColor;
    }

    void Update()
    {
        // Red light if ANY camera is facing the player or turning toward them
        bool isRedLight = false;
        foreach (var cam in _cameras)
        {
            if (cam.State == SurveillanceCamera.CamState.FacingPlayer ||
                cam.State == SurveillanceCamera.CamState.TurningToPlayer)
            {
                isRedLight = true;
                break;
            }
        }

        _targetColor = isRedLight ? redLightColor : normalColor;

        // Smoothly transition all lights toward target color
        foreach (var lt in _lights)
        {
            lt.color = Color.Lerp(lt.color, _targetColor, transitionSpeed * Time.deltaTime);
        }
    }
}