#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IconGenerator : MonoBehaviour {

    [Header("Prefabs")] [Tooltip("IconGenerator will generate icons from these prefabs / objects")]
    public Target[] targets;
    [Tooltip("These images will be applied to EVERY icon generated. Higher index = on top")]
    public Sprite[] overlays;

    [Tooltip("Custom folder used to save the icon. LEAVE EMPTY FOR DEFAULT!")]
    public string customFolder = ""; 

    // --- 新增缩放控制字段 ---
    [Header("Icon Framing")]
    [Tooltip("调整此值以控制相机距离。值越小（如 1.8），相机越近，物体在图标中显得越大。默认值 2.5")]
    public float distancePaddingFactor = 2.5f; 
    // ----------------------

    [Header("Debugging")]
    public Texture rawIcon;
    public Texture2D icon;
    public List<Texture2D> overlayIcons = new List<Texture2D>();

    private Texture2D finalIcon;
    private byte[] overlayBytes;

    private Camera renderCam;
    private Light renderLight;
    private GameObject previewObject;
    private Vector3 spawnPosition = new Vector3(10000, 10000, 10000); 
    
    // 1. 在这里定义了橙色 (#dfad7e)
    private Color orangeColor = new Color(0.875f, 0.678f, 0.494f, 1f);
    
    private const int ICON_SIZE = 256; 

    void Start ()
    {
        GetOverlayTextures();
        int targetCount = 0;
        string iconName;

        if (targets == null || targets.Length == 0)
        {
            Debug.LogError("You need to specify targets!");
            return;
        }

        if (string.IsNullOrEmpty(customFolder) || !Directory.Exists(Application.dataPath + "/" + customFolder)) {
            customFolder = "GeneratedIcons";
            string folderPath = Application.dataPath + "/" + customFolder;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }
        }
        
        SetupVirtualStudio();

        foreach (Target target in targets)
        {
            if (target.obj == null)
            {
                continue;
            }

            GameObject targetObj = target.obj;

            if (previewObject != null) DestroyImmediate(previewObject);
            previewObject = Instantiate(target.obj, spawnPosition, Quaternion.identity); 

            // *** 关键：调用 FrameObject 来调整距离 ***
            FrameObject(renderCam, previewObject); 
            
            icon = CaptureScreenshot(renderCam, ICON_SIZE, ICON_SIZE);

            if (icon == null)
            {
                Debug.LogError("There was an error generating image from " + targetObj.name + ". Are you sure this is an 3D object?");
                continue;
            }

            if (overlayIcons.Count != 0)
            {
                icon = GetFinalTexture(icon, targetCount);
            }

            byte[] bytes = icon.EncodeToPNG ();

            if (IsNullOrWhiteSpace(target.name))
                iconName = targetObj.name;
            else
                iconName = target.name;

            string folderPath = Application.dataPath + "/" + customFolder;
            File.WriteAllBytes (folderPath + "/" + iconName + ".png", bytes);
            Debug.Log ("File saved in: " + folderPath + "/" + iconName + ".png");

            targetCount++;
        }

        Cleanup();
        AssetDatabase.Refresh();
    }

    // --- 相机设置函数 ---
    void SetupVirtualStudio()
    {
        GameObject camObj = new GameObject("IconRenderCamera");
        renderCam = camObj.AddComponent<Camera>();
        renderCam.transform.position = spawnPosition; 

        renderCam.transform.rotation = Quaternion.Euler(30f, 120f, 0f); 
        renderCam.orthographic = false; 
        renderCam.fieldOfView = 45f; 

        renderCam.clearFlags = CameraClearFlags.SolidColor;
        
        // 2. 这里的背景颜色已修改为 orangeColor
        renderCam.backgroundColor = orangeColor; 
        
        renderCam.cullingMask = 1 << 0; 

        GameObject lightObj = new GameObject("IconRenderLight");
        renderLight = lightObj.AddComponent<Light>();
        renderLight.type = LightType.Directional;
        renderLight.intensity = 1.2f;
        renderLight.transform.position = spawnPosition + new Vector3(0, 3, 0);
        renderLight.transform.rotation = Quaternion.Euler(50, -30, 0); 
    }
    
    // --- 缩放距离控制函数 ---
    void FrameObject(Camera cam, GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        Vector3 objectCenter = bounds.center;
        float objectRadius = bounds.extents.magnitude;

        // distancePaddingFactor 越小，distance 越小，相机越近。
        float distance = objectRadius * distancePaddingFactor / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        
        Vector3 direction = cam.transform.forward; 
        cam.transform.position = objectCenter - direction * distance;
        cam.transform.LookAt(objectCenter); 
    }
    
    Texture2D CaptureScreenshot(Camera cam, int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        
        cam.Render();

        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false); 
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt); 

        return screenShot;
    }

    void Cleanup()
    {
        if (renderCam != null) DestroyImmediate(renderCam.gameObject);
        if (renderLight != null) DestroyImmediate(renderLight.gameObject);
        if (previewObject != null) DestroyImmediate(previewObject);
    }

    // --- Overlay 处理 ---
    private void GetOverlayTextures()
    {
        for (int i = 0; i < overlays.Length; i++)
        {
            if (overlays[i] == null) continue;
            string overlayPath = AssetDatabase.GetAssetPath(overlays[i]);
            byte[] fileData = File.ReadAllBytes(overlayPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            overlayIcons.Add(tex);
        }
    }

    private Texture2D GetFinalTexture(Texture2D texture, int id)
    {
        finalIcon = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
        for (int i = 0; i < overlayIcons.Count; i++)
        {
            if(i == 0) CombineTextures(finalIcon, texture, overlayIcons[i]);
            else CombineTextures(finalIcon, finalIcon, overlayIcons[i]);
        }
        return finalIcon;
    }

    public void CombineTextures(Texture2D final, Texture2D image, Texture2D overlay)
    {
        var offset = new Vector2(((final.width - overlay.width) / 2), ((final.height - overlay.height) / 2));
        final.SetPixels(image.GetPixels());
        for (int y = 0; y < overlay.height; y++)
        {
            for (int x = 0; x < overlay.width; x++)
            {
                Color PixelColorFore = overlay.GetPixel(x, y) * overlay.GetPixel(x, y).a;
                Color PixelColorBack = final.GetPixel((int)x + (int)offset.x, y + (int)offset.y) * (1 - PixelColorFore.a);
                final.SetPixel((int)x + (int)offset.x, (int)y + (int)offset.y, PixelColorBack + PixelColorFore);
            }
        }
        final.Apply();
    }

    public bool IsNullOrWhiteSpace(string value)
    {
        if (value != null)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i])) return false;
            }
        }
        return true;
    }
}

[System.Serializable]
public class Target
{
    [Header("If the name value is empty, prefab name will be used as the filename!")]
    public string name;
    public GameObject obj;
}
#endif