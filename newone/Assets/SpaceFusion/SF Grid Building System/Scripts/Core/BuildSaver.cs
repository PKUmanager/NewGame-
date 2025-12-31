using LeanCloud;
using LeanCloud.Storage;
using UnityEngine;

public class BuildSaver : MonoBehaviour
{
    // 单例：为了让别人方便叫它干活
    public static BuildSaver Instance;
    void Awake() { Instance = this; }

    // === 核心功能：上传一个建筑 ===
    // 谁调用它？答：你的 GridBuildingSystem (建造系统)
    public async void SaveOneBuilding(string prefabName, Vector3 pos, float rotY)
    {
        // 1. 检查登录
        LCUser currentUser = await LCUser.GetCurrent();
        if (currentUser == null) return;

        // 2. 打包数据 (写纸条)
        LCObject buildingData = new LCObject("UserStructure");
        buildingData["owner"] = currentUser;   // 记下户主
        buildingData["id"] = prefabName;       // 记下型号
        buildingData["x"] = pos.x;             // 记下位置
        buildingData["z"] = pos.z;
        buildingData["r"] = rotY;

        // 3. 发送！
        try
        {
            await buildingData.Save();
            Debug.Log("✅ 保存成功：" + prefabName);
        }
        catch (LCException e)
        {
            Debug.LogError("❌ 保存失败：" + e.Message);
        }
    }
}