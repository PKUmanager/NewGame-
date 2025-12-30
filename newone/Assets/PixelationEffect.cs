using UnityEngine;

// 要求挂载该脚本的物体必须有Camera组件
[RequireComponent(typeof(Camera))]
public class PixelationEffect : MonoBehaviour
{
    // 公开变量：在Inspector面板赋值我们的材质
    public Material pixelMaterial;

    // 相机渲染完成后执行（Built-in管线后处理核心回调）
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // 如果材质不为空，就将屏幕纹理传入材质，并输出到屏幕
        if (pixelMaterial != null)
        {
            Graphics.Blit(source, destination, pixelMaterial);
        }
        else
        {
            // 材质为空时，直接显示原画面
            Graphics.Blit(source, destination);
        }
    }
}