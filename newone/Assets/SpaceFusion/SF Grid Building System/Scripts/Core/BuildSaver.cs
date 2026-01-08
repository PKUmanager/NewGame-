using UnityEngine;
using LeanCloud;
using LeanCloud.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Managers;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using TMPro;

public class BuildSaver : MonoBehaviour
{
    public static BuildSaver Instance;
    public PlacementHandler placementHandler;

    [Header("提示框设置")]
    public GameObject tipPanel;
    public TMP_Text tipText;

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
            ShowTip("❌ 请先登录！");
            return;
        }

        if (placementHandler == null)
        {
            ShowTip("❌ 系统错误：找不到 PlacementHandler");
            return;
        }

        ShowTip("⏳ 正在清理旧存档...");
        Debug.Log("开始上传流程...");

        try
        {
            // 1. 清理旧数据
            while (true)
            {
                LCQuery<LCObject> query = new LCQuery<LCObject>("UserStructure");
                query.WhereEqualTo("owner", currentUser);
                query.Limit(1000);
                var oldDataList = await query.Find();
                if (oldDataList.Count == 0) break;
                await LCObject.DeleteAll(oldDataList);
            }

            Debug.Log("✅ 云端旧存档已清空");

            // 2. 准备新数据
            var allBuildings = placementHandler.GetAllBuildings();
            List<LCObject> cloudDataList = new List<LCObject>();

            foreach (var info in allBuildings)
            {
                LCObject item = new LCObject("UserStructure");
                item["owner"] = currentUser;
                item["prefabName"] = info.name;

                // 保留世界坐标 (仅供参考或网页展示)
                item["posX"] = info.position.x;
                item["posZ"] = info.position.z;
                item["rotY"] = info.rotation;

                // ★★★ 核心修复：保存绝对网格坐标 (GridX, GridZ) ★★★
                // 这是解决偏移的关键，整数坐标永远准确
                item["gridX"] = info.gridPosition.x;
                item["gridZ"] = info.gridPosition.z;

                cloudDataList.Add(item);

                // 统计
                string safeName = info.name.Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("-", "_");
                currentUser.Increment("stat_" + safeName, 1);
            }

            // 3. 上传云端
            if (cloudDataList.Count > 0)
            {
                await LCObject.SaveAll(cloudDataList);
            }
            await currentUser.Save();

            // ★★★ 核心修复：顺便强制保存一份到本地！ ★★★
            // 这样你下次登录时，本地存档就是最新的，完全不需要去云端拉，避免了加载逻辑的潜在问题
            if (GameManager.Instance != null && GameManager.Instance.saveData != null)
            {
                SaveSystem.Save(GameManager.Instance.saveData);
                Debug.Log("✅ 本地存档已同步更新");
            }

            ShowTip("上传成功！");
            Debug.Log($"上传完成！共 {cloudDataList.Count} 个物体。");

            if (TaskService.Instance != null)
                TaskService.Instance.MarkUploadCompleted();
        }
        catch (LCException e)
        {
            ShowTip(" 上传失败: " + e.Message);
            Debug.LogError("上传出错: " + e.Message);
        }
    }

    void ShowTip(string message)
    {
        if (tipPanel != null && tipText != null)
        {
            tipPanel.SetActive(true);
            tipText.text = message;
            CancelInvoke("HideTip");
            Invoke("HideTip", 2.0f);
        }
    }

    void HideTip()
    {
        if (tipPanel != null) tipPanel.SetActive(false);
    }
}