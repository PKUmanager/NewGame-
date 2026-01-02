using UnityEngine;

[ExecuteInEditMode] // 允许在编辑器不运行的情况下也能看到效果
[RequireComponent(typeof(Camera))]
public class PixelCameraEffect : MonoBehaviour
{
    // 公开变量，方便在面板调节像素块大小
    [Range(1, 100)]
    public float pixelSize = 10.0f;

    private Material pixelMat;
    private Shader pixelShader;

    private void OnEnable()
    {
        // 自动查找刚才创建的Shader
        pixelShader = Shader.Find("Hidden/PixelizeShader");

        if (pixelShader == null)
        {
            Debug.LogError("找不到Shader，请检查Shader名字是否为 Hidden/PixelizeShader");
            enabled = false;
            return;
        }

        // 创建材质球
        if (pixelMat == null)
        {
            pixelMat = new Material(pixelShader);
        }
    }

    // Unity内置管线的后处理核心函数
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (pixelMat != null)
        {
            // 将参数传给Shader
            pixelMat.SetFloat("_PixelSize", pixelSize);

            // 执行后处理：将源画面(source)应用材质(pixelMat)后输出到目标(destination)
            Graphics.Blit(source, destination, pixelMat);
        }
        else
        {
            // 如果材质有问题，直接输出原画面
            Graphics.Blit(source, destination);
        }
    }

    private void OnDisable()
    {
        if (pixelMat != null)
        {
            DestroyImmediate(pixelMat);
        }
    }
}
