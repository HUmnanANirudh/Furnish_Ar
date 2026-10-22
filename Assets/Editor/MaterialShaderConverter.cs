using System.IO;
using UnityEditor;
using UnityEngine;

public class MaterialShaderConverter
{
    [MenuItem("Furnish AR/Convert Materials to Standard Shader")]
    public static void ConvertAllMaterials()
    {
        Shader standardShader = Shader.Find("Standard");
        if (standardShader == null)
        {
            Debug.LogError("Standard shader not found");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Furniture Mega Pack/Materials" });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            Texture mainTex = null;
            if (mat.HasProperty("_MainTex")) mainTex = mat.GetTexture("_MainTex");
            if (mainTex == null && mat.HasProperty("_BaseMap")) mainTex = mat.GetTexture("_BaseMap");
            if (mainTex == null && mat.HasProperty("_BaseTexture")) mainTex = mat.GetTexture("_BaseTexture");

            Texture bumpMap = null;
            if (mat.HasProperty("_BumpMap")) bumpMap = mat.GetTexture("_BumpMap");
            if (bumpMap == null && mat.HasProperty("_NormalTexture")) bumpMap = mat.GetTexture("_NormalTexture");
            if (bumpMap == null && mat.HasProperty("_Normal_Map")) bumpMap = mat.GetTexture("_Normal_Map");

            Texture metallicGloss = null;
            if (mat.HasProperty("_MetallicGlossMap")) metallicGloss = mat.GetTexture("_MetallicGlossMap");
            if (metallicGloss == null && mat.HasProperty("_MetallicTexture")) metallicGloss = mat.GetTexture("_MetallicTexture");
            if (metallicGloss == null && mat.HasProperty("_Metallic_Map")) metallicGloss = mat.GetTexture("_Metallic_Map");

            Color baseColor = Color.white;
            if (mat.HasProperty("_Color")) baseColor = mat.GetColor("_Color");
            else if (mat.HasProperty("_BaseColor")) baseColor = mat.GetColor("_BaseColor");

            mat.shader = standardShader;

            if (mainTex != null) mat.SetTexture("_MainTex", mainTex);
            mat.SetColor("_Color", baseColor);

            if (bumpMap != null)
            {
                mat.SetTexture("_BumpMap", bumpMap);
                mat.EnableKeyword("_NORMALMAP");
            }
            else
            {
                mat.DisableKeyword("_NORMALMAP");
            }

            if (metallicGloss != null)
            {
                mat.SetTexture("_MetallicGlossMap", metallicGloss);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                mat.SetFloat("_GlossMapScale", 1.0f);
            }
            else
            {
                mat.DisableKeyword("_METALLICSPECGLOSSMAP");
            }

            mat.SetFloat("_Glossiness", 0.5f);
            mat.SetFloat("_Metallic", metallicGloss != null ? 1.0f : 0.0f);

            EditorUtility.SetDirty(mat);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Successfully converted " + count + " materials to Built-in Standard shader");
    }
}
