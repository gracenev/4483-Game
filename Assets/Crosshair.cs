using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public int dotSize = 6;
    private Texture2D dotTexture;

    void Start()
    {
        dotTexture = new Texture2D(dotSize, dotSize);
        float radius = dotSize / 2f;
        Color clear = new Color(0, 0, 0, 0);

        for (int x = 0; x < dotSize; x++)
        {
            for (int y = 0; y < dotSize; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                dotTexture.SetPixel(x, y, dist <= radius ? Color.white : clear);
            }
        }

        dotTexture.Apply();
    }

    void OnGUI()
    {
        float x = Screen.width / 2f - dotSize / 2f;
        float y = Screen.height / 2f - dotSize / 2f;
        GUI.DrawTexture(new Rect(x, y, dotSize, dotSize), dotTexture);
    }
}