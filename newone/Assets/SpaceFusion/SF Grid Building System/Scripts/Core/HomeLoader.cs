using UnityEngine;
using LeanCloud.Storage;
using System.Collections.Generic;
using System.Threading.Tasks; // 引用 Task

public class HomeLoader : MonoBehaviour
{
    public static HomeLoader Instance;

    [Header("建筑生成的根节点")]
    public Transform buildingRoot;

    // ==========================================
    // ★★★ 新增：建筑清单 ★★★
    // 请在 Unity Inspector 里，把你 000Prefabs 里的所有建筑都拖进这个列表！
    // ==========================================
    public List<GameObject> buildingList;

    // 私有的字典，用来快速通过名字找到物体
    private Dictionary<string, GameObject> buildingDict;

    void Awake()
    {
        Instance = this;
        InitDictionary(); // 游戏一开始，先把列表转成字典
    }

    // 把 List 转成 Dictionary，方便按名字查找
    void InitDictionary()
    {
        buildingDict = new Dictionary<string, GameObject>();
        foreach (var prefab in buildingList)
        {
            if (prefab != null)
            {
                // 这里的 Key 是预制体的名字 (比如 "Chair")
                if (!buildingDict.ContainsKey(prefab.name))
                {
                    buildingDict.Add(prefab.name, prefab);
                }
            }
        }
    }

    // === 核心功能：加载某人的家 ===
    public async void LoadHome(string targetUsername)
    {
        Debug.Log("正在前往 " + targetUsername + " 的家...");

        // 1. 【拆迁】清空旧建筑
        foreach (Transform child in buildingRoot)
        {
            Destroy(child.gameObject);
        }

        // 2. 【找人】
        LCQuery<LCUser> userQuery = LCUser.GetQuery();
        userQuery.WhereEqualTo("username", targetUsername);
        LCUser targetUser = await userQuery.First();

        if (targetUser == null)
        {
            Debug.Log("查无此人");
            return;
        }

        // 3. 【拿图纸】
        LCQuery<LCObject> buildQuery = new LCQuery<LCObject>("UserStructure");
        buildQuery.WhereEqualTo("owner", targetUser);
        var dataList = await buildQuery.Find();

        // 4. 【施工】
        foreach (var data in dataList)
        {
            // 读数据
            string name = data["id"] as string; // 获取保存的名字，比如 "RedChair"
            float x = System.Convert.ToSingle(data["x"]);
            float z = System.Convert.ToSingle(data["z"]);
            float r = System.Convert.ToSingle(data["r"]);

            // ★★★ 修改点：不再去 Resources 找，而是查字典 ★★★
            GameObject prefab = null;

            if (buildingDict.ContainsKey(name))
            {
                prefab = buildingDict[name];
            }
            else
            {
                Debug.LogWarning("找不到建筑模型：" + name + "，请检查是否拖进了 Building List！");
            }

            // 生成
            if (prefab != null)
            {
                Vector3 pos = new Vector3(x, 0, z);
                Quaternion rot = Quaternion.Euler(0, r, 0);
                Instantiate(prefab, pos, rot, buildingRoot);
            }
        }
        Debug.Log("🏡 加载完毕！");
    }
}