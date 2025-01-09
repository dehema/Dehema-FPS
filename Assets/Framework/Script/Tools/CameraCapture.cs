using UnityEngine;
using System.IO;

public class CameraCapture : MonoBehaviour
{
    [Header("拍照设置")]
    [Tooltip("拍照快捷键")]
    public KeyCode captureKey = KeyCode.F12;
    
    [Tooltip("保存路径")]
    public string savePath = "Screenshots";
    
    [Tooltip("图片尺寸")]
    public Vector2 imageSize = new Vector2(1920, 1080);
    
    [Tooltip("是否使用透明背景")]
    public bool transparentBackground = true;

    private Camera cam;

    void Start()
    {
        // 获取相机组件
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraCapture脚本必须挂载在带有Camera组件的物体上!");
            enabled = false;
            return;
        }

        // 确保保存路径存在
        string fullPath = Path.Combine(Application.dataPath, "..", savePath);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    void Update()
    {
        // 按下快捷键拍照
        if (Input.GetKeyDown(captureKey))
        {
            CaptureImage();
        }
    }

    public void CaptureImage()
    {
        // 创建渲染纹理
        RenderTexture rt = new RenderTexture((int)imageSize.x, (int)imageSize.y, 24);
        rt.format = RenderTextureFormat.ARGB32;
        rt.antiAliasing = 8;

        // 备份当前相机设置
        RenderTexture prevRT = cam.targetTexture;
        CameraClearFlags prevClearFlags = cam.clearFlags;
        Color prevBackgroundColor = cam.backgroundColor;

        // 设置相机用于截图
        cam.targetTexture = rt;
        if (transparentBackground)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
        }

        // 渲染相机画面
        cam.Render();

        // 创建临时渲染纹理来读取像素
        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        screenshot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        screenshot.Apply();

        // 恢复相机设置
        cam.targetTexture = prevRT;
        cam.clearFlags = prevClearFlags;
        cam.backgroundColor = prevBackgroundColor;
        RenderTexture.active = null;

        // 销毁临时渲染纹理
        Destroy(rt);

        // 保存图片
        byte[] bytes = screenshot.EncodeToPNG();
        string filename = Path.Combine(Application.dataPath, "..", savePath, 
            $"Screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
        
        File.WriteAllBytes(filename, bytes);

        // 销毁临时纹理
        Destroy(screenshot);

        Debug.Log($"截图已保存: {filename}");
    }
}
