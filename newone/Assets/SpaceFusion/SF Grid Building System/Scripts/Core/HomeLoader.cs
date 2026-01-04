using UnityEngine;
using LeanCloud.Storage;
using System.Collections.Generic;
using System.Threading.Tasks; // 引用 Task

public class HomeLoader : MonoBehaviour
{
    public static HomeLoader Instance;

    [Header("建筑生成的根节点")]
    public Transform buildingRoot;
    // ★★★ 【新增 1】 把“回我家”按钮拖进来 ★★★
    public GameObject returnHomeButton;

    // ★★★ 【新增 1】 建造系统的总开关 ★★★
    // 请在 Inspector 里把那个挂着 "GridBuildingSystem" 或 "PlacementHandler" 的物体拖进去
    // ==========================================
    public GameObject buildingSystemObject;


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
                          // ★★★ 【新增 2】 游戏刚开始默认隐藏按钮 (默认在自己家) ★★★
        if (returnHomeButton != null)
            returnHomeButton.SetActive(false);
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

    // ★★★ 【新增】 游戏一开始，自动加载自己的家 ★★★
    async void Start()
    {
        // 1. 给 SDK 一点初始化时间
        await System.Threading.Tasks.Task.Delay(500); // 延长到 0.5秒，更稳

        Debug.Log("🔄 [系统启动] 正在检查登录状态...");

        // 2. 获取当前用户
        LCUser currentUser = await LCUser.GetCurrent();

        if (currentUser != null)
        {
            Debug.Log($"👤 检测到用户 [{currentUser.Username}]，开始加载家园...");

            // ★★★ 核心调用 ★★★
            LoadHome(currentUser.Username);
        }
        else
        {
            Debug.LogWarning("⚠️ 未检测到登录用户，家园保持为空。请登录。");
        }
    }


    // === 核心功能：加载某人的家 ===
    public async void LoadHome(string targetUsername)
    {
        Debug.Log("正在前往 " + targetUsername + " 的校园...");



        // ★★★ 【新增 2】 判断是谁家，决定开不开建造 ★★★
        // =========================================================
        LCUser me = await LCUser.GetCurrent();

        if (me != null && buildingSystemObject != null)
        {
            if (me.Username == targetUsername)
            {
                // 情况A：目标名字 = 我的名字 -> 回自己家了
                // 动作：【开启】建造系统
                buildingSystemObject.SetActive(true);

                // 动作：隐藏“回我家”按钮
                if (returnHomeButton) returnHomeButton.SetActive(false);
            }
            else
            {
                // 情况B：目标名字 != 我的名字 -> 去别人家了
                // 动作：【关闭】建造系统 (拔电源！)
                buildingSystemObject.SetActive(false);

                // 动作：显示“回我家”按钮
                if (returnHomeButton) returnHomeButton.SetActive(true);
            }
        }
        // =========================================================

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
        buildQuery.Include("owner");
        var dataList = await buildQuery.Find();

        if (dataList.Count > 0)
        {
            // 取出第一条数据，看看它的主人是谁
            LCUser owner = dataList[0]["owner"] as LCUser;
            Debug.Log("正在参观：" + owner.Username+"的校园");

            // 你以后可以在这里写：TitleText.text = owner.Username + " 的家";
        }
        else
        {
            Debug.Log("这个校园空荡荡的");
        }

        // ★★★ 【新增步骤 A】 开始生成前，先告诉经理：把计数器归零！ ★★★
        // =========================================================
        if (NPCManager.Instance != null)
        {
            NPCManager.Instance.ClearCounts();
        }
        // =========================================================

        // 4. 【施工】
        foreach (var data in dataList)
        {
            // 读数据
            string name = data["prefabName"] as string; // 获取保存的名字，比如 "RedChair"
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
                // ★★★ 【新增步骤 B】 生成前，看一眼这个建筑是什么类型，让经理记下来 ★★★
                // =========================================================

                // 获取预制体身上的“身份证”脚本
                var attr = prefab.GetComponent<BuildingAttribute>();

                // 如果有身份证，且经理在场，就增加计数
                if (attr != null && NPCManager.Instance != null)
                {
                    // 注意：这里我们只增加计数，不立刻刷新NPC（为了性能，等循环完了再统一刷新）
                    // 所以我在 NPCManager 里写了 AddBuildingCount，它会自动累加
                    NPCManager.Instance.AddBuildingCount(attr.type);
                }
                // =========================================================

                // 生成物体
                Vector3 pos = new Vector3(x, 0, z);
                Quaternion rot = Quaternion.Euler(0, r, 0);
                Instantiate(prefab, pos, rot, buildingRoot);
            }
        }
        Debug.Log("🏡 加载完毕！");
    }
}