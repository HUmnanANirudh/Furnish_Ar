using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CatalogThumbnailBaker
{
    private static readonly Color BgColor = new Color(0.973f, 0.965f, 0.945f, 1f);

    [MenuItem("Furnish AR/Bake All Catalog Thumbnails")]
    public static void BakeAllThumbnails()
    {
        Dictionary<string, string> categories = new Dictionary<string, string>
        {
            { "Sofas", "Assets/Furniture Mega Pack/Prefabs/Sofas" },
            { "Chairs", "Assets/Furniture Mega Pack/Prefabs/Chairs" },
            { "Beds", "Assets/Furniture Mega Pack/Prefabs/Beds" },
            { "Tables", "Assets/Furniture Mega Pack/Prefabs/Tables" },
            { "Closets", "Assets/Furniture Mega Pack/Prefabs/Closets" },
            { "Drawers", "Assets/Furniture Mega Pack/Prefabs/Drawers" },
            { "Bathroom", "Assets/Furniture Mega Pack/Prefabs/Bathroom" },
            { "Kitchen", "Assets/Furniture Mega Pack/Prefabs/Kitchen" },
            { "Cushions", "Assets/Furniture Mega Pack/Prefabs/Cushioins" }
        };

        foreach (var pair in categories)
        {
            BakeCategory(pair.Key, pair.Value, 8);
        }

        BakePrefab("Assets/Assets/Furnitures/White Sofa/White Sofa.prefab", "Assets/Resources/Textures/Catalog/Sofas/WhiteSofa.png");
        BakePrefab("Assets/Assets/Furnitures/Brown Sofa/Brown Sofa.prefab", "Assets/Resources/Textures/Catalog/Sofas/BrownSofa.png");
        BakePrefab("Assets/Assets/Furnitures/Light Brown Sofa/Light Brown Sofa.prefab", "Assets/Resources/Textures/Catalog/Sofas/LightBrownSofa.png");

        AssetDatabase.Refresh();
        Debug.Log("Catalog thumbnail baking complete");
    }

    private static void BakeCategory(string catName, string folderPath, int maxItems)
    {
        if (!Directory.Exists(folderPath)) return;

        string outDir = Path.Combine("Assets/Resources/Textures/Catalog", catName);
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        string[] files = Directory.GetFiles(folderPath, "*.prefab");
        System.Array.Sort(files);

        int count = Mathf.Min(maxItems, files.Length);
        for (int i = 0; i < count; i++)
        {
            string pPath = files[i];
            string baseName = Path.GetFileNameWithoutExtension(pPath);
            string outFile = Path.Combine(outDir, baseName + ".png");
            if (!File.Exists(outFile))
            {
                BakePrefab(pPath, outFile);
            }
        }
    }

    private static void BakePrefab(string prefabPath, string outputPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return;

        GameObject go = Object.Instantiate(prefab);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.Euler(15f, 225f, 0f);

        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Object.DestroyImmediate(go);
            return;
        }

        Bounds bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
        foreach (Renderer r in renderers)
        {
            if (r.enabled) bounds.Encapsulate(r.bounds);
        }

        GameObject camObj = new GameObject("BakeCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgColor;

        float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (maxDim <= 0.01f) maxDim = 1f;
        float distance = maxDim * 2.3f;
        camObj.transform.position = bounds.center + new Vector3(distance * 0.7f, distance * 0.55f, -distance * 0.7f);
        camObj.transform.LookAt(bounds.center);

        GameObject lightObj = new GameObject("BakeLight");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1.25f;
        lightObj.transform.rotation = Quaternion.Euler(45f, 45f, 0f);

        GameObject fillLightObj = new GameObject("FillLight");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.color = new Color(0.9f, 0.95f, 1f);
        fillLight.intensity = 0.65f;
        fillLightObj.transform.rotation = Quaternion.Euler(-30f, -135f, 0f);

        RenderTexture rt = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(256, 256, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        tex.Apply();

        byte[] pngBytes = tex.EncodeToPNG();
        string dir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(outputPath, pngBytes);

        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(camObj);
        Object.DestroyImmediate(lightObj);
        Object.DestroyImmediate(fillLightObj);
        Object.DestroyImmediate(go);
    }
}
