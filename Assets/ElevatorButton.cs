using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorButton : MonoBehaviour
{
    [Tooltip("Full asset path of the scene to load, e.g. Assets/Scenes/Floor 8.unity")]
    public string targetScene;

    [Tooltip("Seconds to wait after turning green before loading the next scene")]
    public float loadDelay = 1.5f;

    public Color successColor = Color.green;

    public bool locked = false;

    private Renderer rend;
    private bool activated = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    public void Press()
    {
        if (locked || activated) return;

        activated = true;
        rend.material.color = successColor;
        Debug.Log($"Elevator button pressed — loading {targetScene} in {loadDelay}s");
        Invoke(nameof(LoadNextScene), loadDelay);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(targetScene);
    }
}
