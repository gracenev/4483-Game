using UnityEngine;

// Attach this to the fuse box on the elevator wall.
// When the player clicks on it, it starts the Simon Says circuit game.
public class FuseBoxInteractable : MonoBehaviour
{
    private SimonSaysGame _game;

    void Start()
    {
        _game = FindAnyObjectByType<SimonSaysGame>();
        if (_game == null)
            Debug.LogWarning("FuseBoxInteractable couldn't find SimonSaysGame in the scene.");
    }

    // Called by PlayerInteract when the player looks at the fuse box and clicks
    public void Interact()
    {
        _game?.StartGame();
    }
}