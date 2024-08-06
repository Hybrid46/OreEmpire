using UnityEngine;

public class GradientTextureGenerator : Singleton<GradientTextureGenerator>
{
    public Gradient gradient;
    [Range(16, 1024)] public int textureResolution = 64;
    public Texture2D texture;

    void Start()
    {
        UpdateTexture();
    }

    public void UpdateTexture()
    {
        texture = new Texture2D(textureResolution, 1, TextureFormat.RGBA32, false);

        if (gradient != null)
        {
            Color[] colours = new Color[texture.width];
            for (int i = 0; i < textureResolution; i++)
            {
                Color gradientCol = gradient.Evaluate(i / (textureResolution - 1f));
                colours[i] = gradientCol;
            }

            texture.SetPixels(colours);
            texture.Apply();
        }
    }
}