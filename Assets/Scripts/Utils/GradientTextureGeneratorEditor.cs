using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GradientTextureGenerator))]
public class GradientTextureGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GradientTextureGenerator gradientTextureGenerator = (GradientTextureGenerator)target;

        if (GUILayout.Button("Update Texture")) gradientTextureGenerator.UpdateTexture();

        if (gradientTextureGenerator.texture != null && GUILayout.Button("Save Texture as Asset"))
        {
            SaveTextureAsAsset(gradientTextureGenerator.texture);
        }
    }

    private void SaveTextureAsAsset(Texture2D texture)
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Texture as PNG",
            "GradientTexture.png",
            "png",
            "Please enter a file name to save the texture to"
        );

        if (!string.IsNullOrEmpty(path))
        {
            byte[] pngData = texture.EncodeToPNG();

            System.IO.File.WriteAllBytes(path, pngData);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.SaveAndReimport();
            }

            // Refresh the AssetDatabase
            AssetDatabase.Refresh();
        }
    }
}