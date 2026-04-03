using UnityEngine;
 
public class KeySpawner : MonoBehaviour
{
    [Tooltip("The key GameObject to reposition")]
    public GameObject keyObject;
 
    [Tooltip("Height off the floor the key floats at")]
    public float spawnHeight = 1.19f;
 
    // Room floor centres — first two rooms excluded intentionally
    private static readonly Vector3[] RoomCentres = new Vector3[]
    {
        // Room_Left_1 and Room_Right_1 excluded — key never spawns there
        new Vector3(-11f, 0f, 42.5f),      // Room_Left_2
        new Vector3( 11f, 0f, 42.5f),      // Room_Right_2
        new Vector3(-11f, 0f, 70.8333f),   // Room_Left_3
        new Vector3( 11f, 0f, 70.8333f),   // Room_Right_3
    };
 
    // Candidate offsets tried within the room (XZ relative to room centre)
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
            Debug.LogError("[KeySpawner] Key Object not assigned!", this);
            return;
        }
 
        // Shuffle rooms for true randomness
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
 
                // If nothing overlaps this spot, it's clear of furniture
                if (!Physics.CheckSphere(candidate, 0.35f))
                {
                    chosenPos = candidate;
                    found     = true;
                    break;
                }
            }
            if (found) break;
        }
 
        // Fallback — elevate above any furniture if every spot was blocked
        if (!found)
        {
            chosenPos = new Vector3(rooms[0].x, spawnHeight + 2f, rooms[0].z);
            Debug.LogWarning("[KeySpawner] No clear spot found — elevating key above room.");
        }
 
        keyObject.transform.position = chosenPos;
        Debug.Log($"[KeySpawner] Key placed at {chosenPos}");
    }
}