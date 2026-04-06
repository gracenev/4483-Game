using UnityEngine;

/// <summary>
/// Attach to the FuseBox GameObject.
/// Your PlayerInteract raycast script calls Interact() when the player
/// looks at it and presses the interact key.
/// </summary>
public class FuseBoxInteractable : MonoBehaviour
{
    private SimonSaysGame _game;

    void Start()
    {
        _game = FindAnyObjectByType<SimonSaysGame>();
        if (_game == null)
            Debug.LogWarning("[FuseBox] SimonSaysGame not found in scene!");
    }

    public void Interact()
    {
        _game?.StartGame();
    }
}