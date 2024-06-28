using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EditorTool
{
    [MenuItem("Assets/CustomTool/MergeSprite")]
    public static void MergeSprite()
    {
        string[] sprteGUIDs = Selection.assetGUIDs;
        if (sprteGUIDs == null || sprteGUIDs.Length <= 1) return;
        List<string> spritePathList = new List<string>(sprteGUIDs.Length);
        for (int i = 0; i < sprteGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(sprteGUIDs[i]);
            spritePathList.Add(assetPath);
        }
        spritePathList.Sort();

        Texture2D firstTex = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePathList[0]);
        int unitHieght = firstTex.height;
        int unitWidth = firstTex.width;

        Texture2D outputTex = new Texture2D(unitWidth * spritePathList.Count, unitHieght);
        for (int i = 0; i < spritePathList.Count; i++)
        {
            Texture2D temp = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePathList[i]);
            Color[] colors = temp.GetPixels();
            outputTex.SetPixels(i * unitWidth, 0, unitWidth, unitHieght, colors);
        }

        byte[] bytes = outputTex.EncodeToPNG();
        File.WriteAllBytes(spritePathList[0].Remove(spritePathList[0].LastIndexOf(firstTex.name)) + "MergeSprite.png", bytes);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
