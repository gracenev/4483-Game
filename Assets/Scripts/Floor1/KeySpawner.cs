using UnityEngine;

// Randomly places the key in one of the eligible rooms when the scene loads.
// It checks for furniture before placing so the key never ends up inside a desk or shelf.
public class KeySpawner : MonoBehaviour
{
    [Tooltip("Drag the key object here")]
    public GameObject keyObject;

    [Tooltip("How high off the floor the key should float")]
    public float spawnHeight = 1.19f;

    // I'm skipping the first two rooms intentionally - want the player to have to walk a bit
    private static readonly Vector3[] RoomCentres = new Vector3[]
    {
        new Vector3(-11f, 0f, 42.5f),      // Room_Left_2
        new Vector3( 11f, 0f, 42.5f),      // Room_Right_2
        new Vector3(-11f, 0f, 70.8333f),   // Room_Left_3
        new Vector3( 11f, 0f, 70.8333f),   // Room_Right_3
    };

    // Different spots to try inside each room in case something is in the way
    private static readonly Vector2[] SpawnOffsets = new Vector2[]
    {
        new Vector2( 0f,    0f),
        new Vector2( 1.5f,  0f),
        new Vector2(-1.5f,  0f),
        new Vector2( 0f,    1.5f),
        new Vector2( 0f,   -1.5f),
        new Vector2( 2.5f,  2.0f),
        new Vector2(-2.5f,  2.0f),
        new Vector2( 2.5f, -2.0f),
        new Vector2(-2.5f, -2.0f),
    };

    void Start()
    {
        if (keyObject == null)
        {
            Debug.LogError("Key object isn't assigned on the KeySpawner!", this);
            return;
        }

        // Shuffle the room order so it's different every time
        Vector3[] rooms = (Vector3[])RoomCentres.Clone();
        for (int i = rooms.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (rooms[i], rooms[j]) = (rooms[j], rooms[i]);
        }

        Vector3 chosenPos = Vector3.zero;
        bool    found     = false;

        foreach (Vector3 room in rooms)
        {
            foreach (Vector2 offset in SpawnOffsets)
            {
                Vector3 candidate = new Vector3(
                    room.x + offset.x,
                    spawnHeight,
                    room.z + offset.y);

                // Use a small sphere check to make sure nothing is already there
                if (!Physics.CheckSphere(candidate, 0.35f))
                {
                    chosenPos = candidate;
                    found     = true;
                    break;
                }
            }
            if (found) break;
        }

        // If every spot was blocked just float the key above the room centre
        if (!found)
        {
            chosenPos = new Vector3(rooms[0].x, spawnHeight + 2f, rooms[0].z);
            Debug.LogWarning("Couldn't find a clear spot for the key, placing it above the room instead.");
        }

        keyObject.transform.position = chosenPos;
        Debug.Log($"Key placed at {chosenPos}");
    }
}