using UnityEngine;

// 加上这一行，让你不用运行游戏，在编辑器里拖滑块也能看到效果
[ExecuteAlways]
public class PixelizerURP : MonoBehaviour
{
    // header 可以让面板上显示中文提示
    [Header("【第一步】把 PixelRT (黑色方块) 拖到这里")]
    public RenderTexture targetRenderTexture;

    [Header("【第二步】像素倍率 (拖动滑块调节)")]
    // Range 就是让它变成滑块的关键！(最小值1，最大值100)
    [Range(1, 100)]
    public int pixelScale = 5;

    // 下面这三个变量是用来检测是否有变化的，不需要要在面板显示
    private int _lastScale = -1;
    private int _lastScreenWidth = -1;
    private int _lastScreenHeight = -1;

    void Update()
    {
        // 1. 安全检查：如果没拖RenderTexture，直接不执行，防止报错
        if (targetRenderTexture == null) return;

        // 2. 只有当：滑块数值变了 OR 屏幕分辨率变了，才执行计算
        // 这样非常节省电脑性能
        if (pixelScale != _lastScale || Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            UpdateResolution();
        }
    }

    void UpdateResolution()
    {
        // 确保倍数至少是1
        if (pixelScale < 1) pixelScale = 1;

        // 计算目标分辨率 = 屏幕分辨率 / 倍率
        int newWidth = Screen.width / pixelScale;
        int newHeight = Screen.height / pixelScale;

        // 防止分辨率太小导致报错 (最小设为2x2)
        if (newWidth < 2) newWidth = 2;
        if (newHeight < 2) newHeight = 2;

        // 3. 释放旧的内存
        targetRenderTexture.Release();

        // 4. 设置新的宽高
        targetRenderTexture.width = newWidth;
        targetRenderTexture.height = newHeight;

        // 5. 关键：必须设为 Point 模式，不然画面是模糊的而不是马赛克
        targetRenderTexture.filterMode = FilterMode.Point;

        // 更新记录，以便下一帧比对
        _lastScale = pixelScale;
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;

        // (可选) 控制台打印一下，让你知道它在工作
        // Debug.Log($"像素分辨率已刷新: {newWidth} x {newHeight}");
    }
}