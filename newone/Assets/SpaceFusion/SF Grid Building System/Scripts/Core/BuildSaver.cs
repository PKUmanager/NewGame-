using UnityEngine;
using LeanCloud;           // 【修复错误1】 必须加这个！
using LeanCloud.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;

public class BuildSaver : MonoBehaviour
{
    public static BuildSaver Instance;

    public PlacementHandler placementHandler;

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
            Debug.LogError("请先登录！");
            return;
        }

        if (placementHandler == null)
        {
            Debug.LogError("找不到 PlacementHandler！");
            return;
        }

        Debug.Log("⏳ 正在上传数据...");

        var allBuildings = placementHandler.GetAllBuildings();
        Debug.Log($"检测到 {allBuildings.Count} 个建筑，准备上传...");

        try
        {
            // =========================================================
            // 【修复错误3】 DeleteAll 的正确写法
            // =========================================================
            // 1. 建立查询
            LCQuery<LCObject> query = new LCQuery<LCObject>("UserStructure");
            query.WhereEqualTo("owner", currentUser);

            // 2. 先把旧数据全部找出来
            var oldDataList = await query.Find();

            // 3. 如果有旧数据，就批量删除
            if (oldDataList.Count > 0)
            {
                await LCObject.DeleteAll(oldDataList);
                Debug.Log("旧存档已清理...");
            }
            // =========================================================

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

                // ★★★ 【修复】 给名字“洗澡”，去掉非法字符 ★★★
                // =========================================================

                // 1. 把空格、括号、横杠全部替换成下划线或者空字符
                string safeName = info.name
                    .Replace(" ", "_")   // 空格 -> 下划线
                    .Replace("(", "")    // 左括号 -> 删掉
                    .Replace(")", "")    // 右括号 -> 删掉
                    .Replace("-", "_");  // 横杠 -> 下划线

                // 2. 使用“洗干净”的名字来做统计 Key
                currentUser.Increment("stat_" + safeName, 1);

                // =========================================================
            }

            if (cloudDataList.Count > 0)
            {
                await LCObject.SaveAll(cloudDataList);
            }

            await currentUser.Save();

            Debug.Log("✅ 上传完成！");
        }
        catch (LCException e)
        {
            Debug.LogError("❌ 上传失败: " + e.Message);
        }
    }
}