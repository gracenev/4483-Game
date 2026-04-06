using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Transitions hallway lights to red when CameraSync is in red state.
/// </summary>
public class RedLightEffect : MonoBehaviour
{
    [Header("Colors")]
    public Color normalColor   = new Color(1f, 0.95f, 0.85f);
    public Color redLightColor = new Color(1f, 0.05f, 0.05f);

    [Header("Transition")]
    public float transitionSpeed = 4f;

    private CameraSync _sync;
    private Light[]    _lights;

    void Start()
    {
        _sync = FindAnyObjectByType<CameraSync>();
        if (_sync == null)
            Debug.LogWarning("[RedLightEffect] CameraSync not found!");

        var lights = new List<Light>();
        foreach (Light lt in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (lt.type == LightType.Point)
                lights.Add(lt);
        _lights = lights.ToArray();

        Debug.Log($"[RedLightEffect] Tracking {_lights.Length} lights.");
    }

    void Update()
    {
        if (_sync == null) return;

        bool isRed = _sync.State == CameraSync.CamState.FacingPlayer ||
                     _sync.State == CameraSync.CamState.TurningToPlayer;

        Color target = isRed ? redLightColor : normalColor;

        foreach (var lt in _lights)
            lt.color = Color.Lerp(lt.color, target, transitionSpeed * Time.deltaTime);
    }
}