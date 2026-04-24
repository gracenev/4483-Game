using UnityEngine;

// Placed on the outdoor platform win trigger.
// When the player steps onto the platform the level ends.
public class MazeExitTrigger : MonoBehaviour
{
    private MazeLevelManager _manager;

    void Start()
    {
        _manager = FindAnyObjectByType<MazeLevelManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _manager?.OnPlayerEscaped();
    }
}