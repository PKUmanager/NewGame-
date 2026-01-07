using UnityEngine;
using System.Collections;
using System.Collections.Generic; // 必须引用这个，才能用 List
using LeanCloud.Storage;
using LeanCloud;

public class ScreenshotManager : MonoBehaviour
{
    [Header("1. 主画布 (可选，如果不拖就不隐藏整个画布)")]
    public Canvas mainCanvas;

    [Header("2. 其他散落在各处的按钮/UI (拖进列表)")]
    // ★★★ 【新增】 黑名单列表：截图时隐藏，截完恢复 ★★★
    public List<GameObject> uiElementsToHide;

    // 绑定到按钮点击
    public void OnClickTakeSnapshot()
    {
        StartCoroutine(CaptureRoutine());
    }

    IEnumerator CaptureRoutine()
    {
        // =================================================
        // ★★★ 阶段 A：隐藏所有不该出现的东西 ★★★
        // =================================================

        // 1. 隐藏主画布 (如果拖了的话)
        if (mainCanvas != null) mainCanvas.enabled = false;

        // 2. 隐藏列表里的散装按钮
        foreach (var obj in uiElementsToHide)
        {
            if (obj != null) obj.SetActive(false);
        }

        // =================================================
        // ★★★ 阶段 B：截图 ★★★
        // =================================================

        // 等待当前帧渲染结束 (确保它们真的消失了)
        yield return new WaitForEndOfFrame();

        // 抓取全屏
        Texture2D screenImage = ScreenCapture.CaptureScreenshotAsTexture();

        // =================================================
        // ★★★ 阶段 C：恢复显示 ★★★
        // =================================================

        // 1. 恢复主画布
        if (mainCanvas != null) mainCanvas.enabled = true;

        // 2. 恢复列表里的散装按钮
        foreach (var obj in uiElementsToHide)
        {
            if (obj != null) obj.SetActive(true);
        }

        // =================================================
        // ★★★ 阶段 D：处理数据与上传 ★★★
        // =================================================

        // 转成 JPG
        byte[] imageBytes = screenImage.EncodeToJPG(75);
        Destroy(screenImage);

        Debug.Log("📸 截图完成，准备上传...");
        UploadTask(imageBytes);
    }

    async void UploadTask(byte[] data)
    {
        LCUser currentUser = await LCUser.GetCurrent();
        if (currentUser == null) return;

        try
        {
            string fileName = "Snap_" + System.DateTime.Now.ToString("MMdd_HHmmss") + ".jpg";
            LCFile file = new LCFile(fileName, data);
            await file.Save();

            LCObject record = new LCObject("PlayerScreenshot");
            record["owner"] = currentUser;
            record["image"] = file;
            record["location"] = "MyHome_Preview";

            await record.Save();

            Debug.Log("✅ 截图已上传到云端！");
        }
        catch (LCException e)
        {
            Debug.LogError("上传失败: " + e.Message);
        }
    }
}