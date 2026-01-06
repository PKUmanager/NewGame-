using UnityEngine;
using LeanCloud;           // 【修复错误1】 必须加这个！
using LeanCloud.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using TMPro; // 引用 TMP

public class BuildSaver : MonoBehaviour
{
    public static BuildSaver Instance;

    public PlacementHandler placementHandler;

    // ★★★ 【新增 1】 提示框 UI ★★★
    [Header("提示框设置")]
    public GameObject tipPanel;  // 拖入 TipPanel
    public TMP_Text tipText;     // 拖入 TipText




    void Awake()
    {
        Instance = this;
        if (placementHandler == null)
            placementHandler = FindObjectOfType<PlacementHandler>();
    }

    public async void OnUploadButtonClick()
    {
        LCUser currentUser = await LCUser.GetCurrent();
        if (currentUser == null)
        {
            // 【修改】 没登录时弹窗提示
            ShowTip("❌ 请先登录！");
            Debug.LogError("请先登录！");
            return;
        }

        if (placementHandler == null)
        {
            Debug.LogError("找不到 PlacementHandler！");
            return;
        }

        // 【修改】 开始上传时弹窗提示
        ShowTip("⏳ 正在上传数据...");
        Debug.Log("⏳ 正在上传数据...");

        var allBuildings = placementHandler.GetAllBuildings();

        try
        {
            // 1. 建立查询
            LCQuery<LCObject> query = new LCQuery<LCObject>("UserStructure");
            query.WhereEqualTo("owner", currentUser);

            // ★★★ 关键：这一步是为了防止堆积 ★★★
            // 为了保险，把 Limit 设大一点，确保能把旧的都查出来删掉
            query.Limit(1000);

            // 2. 先把旧数据全部找出来
            var oldDataList = await query.Find();

            // 3. 如果有旧数据，就批量删除
            if (oldDataList.Count > 0)
            {
                await LCObject.DeleteAll(oldDataList);
            }

            List<LCObject> cloudDataList = new List<LCObject>();

            foreach (var info in allBuildings)
            {
                LCObject item = new LCObject("UserStructure");
                item["owner"] = currentUser;
                item["prefabName"] = info.name;
                item["posX"] = info.position.x;
                item["posZ"] = info.position.z;
                item["rotY"] = info.rotation;

                cloudDataList.Add(item);

                // 名字清洗逻辑
                string safeName = info.name
                    .Replace(" ", "_")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("-", "_");

                currentUser.Increment("stat_" + safeName, 1);
            }

            if (cloudDataList.Count > 0)
            {
                await LCObject.SaveAll(cloudDataList);
            }

            await currentUser.Save();

            // =================================================
            // ★★★ 【修改】 成功时弹窗！ ★★★
            // =================================================
            ShowTip("上传成功！");
            Debug.Log("上传完成！");

            if (TaskService.Instance != null)
                TaskService.Instance.MarkUploadCompleted();
        }
        catch (LCException e)
        {
            // =================================================
            // ★★★ 【修改】 失败时弹窗！ ★★★
            // =================================================
            ShowTip("❌ 上传失败: " + e.Message);
            Debug.LogError("❌ 上传失败: " + e.Message);
        }
    }

    // ★★★ 【新增 2】 控制提示框自动消失 ★★★
    void ShowTip(string message)
    {
        if (tipPanel != null && tipText != null)
        {
            tipPanel.SetActive(true); // 显示
            tipText.text = message;   // 改字

            // 停止之前的倒计时，重新开始
            CancelInvoke("HideTip");
            Invoke("HideTip", 2.0f);  // 2秒后自动执行 HideTip
        }
        else
        {
            Debug.Log("提示(未绑定UI): " + message);
        }
    }

    void HideTip()
    {
        if (tipPanel != null) tipPanel.SetActive(false);
    }

}