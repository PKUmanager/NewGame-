using UnityEngine;

[ExecuteAlways]
public class PixelEffectManager : MonoBehaviour
{
    [Header("核心设置")]
    // 1. 把 Quad 拖到这里，这样我们就能控制它的开关和层级
    public GameObject quadObject;

    [Header("效果参数")]
    [Range(1f, 200f)]
    public float pixelSize = 60f;

    [Header("层级排序 (越小越靠后)")]
    // 2. 建议设置为 10。背景设为 0，NPC 设为 20。
    public int sortingOrder = 10;

    private Renderer _quadRenderer;

    void Update()
    {
        // 实时更新 Shader 参数
        Shader.SetGlobalFloat("_PixelSize", pixelSize);

        // 实时更新层级（防止你在运行中改了数值没反应）
        UpdateSorting();
    }

    void UpdateSorting()
    {
        if (quadObject != null)
        {
            if (_quadRenderer == null)
                _quadRenderer = quadObject.GetComponent<Renderer>();

            if (_quadRenderer != null)
            {
                // 强行修改 MeshRenderer 的排序层级
                _quadRenderer.sortingOrder = sortingOrder;
            }
        }
    }

    // --- 下面是给按钮或事件调用的公开方法 ---

    // 开启滤镜
    public void EnableEffect()
    {
        if (quadObject != null) quadObject.SetActive(true);
    }

    // 关闭滤镜
    public void DisableEffect()
    {
        if (quadObject != null) quadObject.SetActive(false);
    }

    // 切换开关状态
    public void ToggleEffect()
    {
        if (quadObject != null) quadObject.SetActive(!quadObject.activeSelf);
    }
}
