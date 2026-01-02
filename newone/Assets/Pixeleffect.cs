using UnityEngine;

// 确保编辑模式下实时预览效果
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))] // 自动关联相机组件
public class PixelEffect : MonoBehaviour
{
    // 绑定像素化材质（保持原有拖拽逻辑）
    public Material effectMaterial;

    // 横向像素数量（列数），数值越大，像素块越小，效果越细腻
    [Header("像素密度控制（数值越大，像素风越弱）")]
    [Range(64, 1024)] // 扩大上限，支持更细腻的效果
    public float pixelColumns = 512;

    // 纵向像素数量（行数），自动适配屏幕比例（可手动调整）
    [Range(64, 1024)]
    public float pixelRows = 288;

    // 像素平滑度：0=完全像素化，1=接近原图，实现精细过渡
    [Header("像素强度过渡（0=最强像素风，1=最弱像素风）")]
    [Range(0f, 1f)]
    public float pixelSmoothness = 0f;

    // 屏幕后处理核心方法
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // 安全校验：材质为空时透传原图
        if (effectMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // 1. 传递参数给Shader（横向/纵向像素数 + 平滑度）
        effectMaterial.SetFloat("_PixelColumns", pixelColumns);
        effectMaterial.SetFloat("_PixelRows", pixelRows);
        effectMaterial.SetFloat("_PixelSmoothness", pixelSmoothness);
        // 传递原始屏幕分辨率，用于Shader内计算
        effectMaterial.SetVector("_ScreenResolution", new Vector2(source.width, source.height));

        // 2. 执行像素化处理
        Graphics.Blit(source, destination, effectMaterial);
    }

    // 编辑器内参数校验，防止异常值
    private void OnValidate()
    {
        // 确保像素数不低于最小值，避免效果异常
        pixelColumns = Mathf.Max(64, pixelColumns);
        pixelRows = Mathf.Max(64, pixelRows);
        // 平滑度限制在0-1之间
        pixelSmoothness = Mathf.Clamp01(pixelSmoothness);
    }
}