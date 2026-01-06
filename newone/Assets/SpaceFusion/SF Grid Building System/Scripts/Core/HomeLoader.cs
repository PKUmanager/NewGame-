using UnityEngine;
using LeanCloud.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HomeLoader : MonoBehaviour
{
    public static HomeLoader Instance;

    [Header("建筑生成的根节点")]
    public Transform buildingRoot;
    public GameObject returnHomeButton;

    // 建造系统的总开关
    public GameObject buildingSystemObject;

    [Header("主界面 UI 控制")]
    public GameObject mainBuildBtn;        // Building_Btn (建造)
    public GameObject mainVisitPreviewBtn; // Preview_Btn_Visit (进入预览)
    public GameObject mainDiscoveryBtn;    // Discovery_Btn (发现)

    [Header("预览界面内部 UI 控制")]
    public GameObject previewConfirmBtn;   // 开始建造/确定 (主人用)
    public GameObject previewExitBtn;      // 退出预览 (客人用)

    // =========================================================
    // ★★★ 【新增】 相机控制变量 ★★★
    // =========================================================
    [Header("相机控制")]
    public Transform mainCameraRig; // 请在 Inspector 里把你的 CameraSystem 或 Main Camera 拖进来
    private Vector3 defaultCameraPos; // 记录初始位置
    private Quaternion defaultCameraRot; // 记录初始旋转
    // =========================================================

    public List<GameObject> buildingList;
    private Dictionary<string, GameObject> buildingDict;

    // ★★★ 【新增 1】 把挂着 TopBarUI_Legacy 的物体拖进来 ★★★
    public TopBarUI_Legacy topBarUI;

    void Awake()
    {
        Instance = this;
        InitDictionary();
        if (returnHomeButton != null)
            returnHomeButton.SetActive(false);
    }

    void InitDictionary()
    {
        buildingDict = new Dictionary<string, GameObject>();
        foreach (var prefab in buildingList)
        {
            if (prefab != null && !buildingDict.ContainsKey(prefab.name))
            {
                buildingDict.Add(prefab.name, prefab);
            }
        }
    }

    async void Start()
    {
        // =========================================================
        // ★★★ 【新增】 记住游戏刚开始时相机的默认位置 ★★★
        // =========================================================
        if (mainCameraRig != null)
        {
            defaultCameraPos = mainCameraRig.position;
            defaultCameraRot = mainCameraRig.rotation;
        }
        // =========================================================

        await System.Threading.Tasks.Task.Delay(500);
        LCUser currentUser = await LCUser.GetCurrent();

        if (currentUser != null)
        {
            LoadHome(currentUser.Username);
        }
    }

    public async void LoadHome(string targetUsername)
    {
        // ★★★ 【新增 2】 加载家园时，顺便更新左上角的名字！ ★★★
        // =========================================================
        if (topBarUI != null)
        {
            topBarUI.UpdateName(targetUsername);
        }
        // =========================================================

        Debug.Log("正在前往 " + targetUsername + " 的家...");
        LCUser me = await LCUser.GetCurrent();

        if (me != null && buildingSystemObject != null)
        {
            if (me.Username == targetUsername)
            {
                // === 情况 A: 回到自己家 (主人模式) ===
                buildingSystemObject.SetActive(true);
                if (returnHomeButton) returnHomeButton.SetActive(false);

                // 1. 主界面按钮状态
                if (mainBuildBtn) mainBuildBtn.SetActive(true);
                if (mainDiscoveryBtn) mainDiscoveryBtn.SetActive(true);
                if (mainVisitPreviewBtn) mainVisitPreviewBtn.SetActive(false);

                // 2. 预览界面内部按钮状态
                if (previewConfirmBtn) previewConfirmBtn.SetActive(true);
                if (previewExitBtn) previewExitBtn.SetActive(false);

                // =========================================================
                // ★★★ 【新增】 回家时，强制复位相机视角 ★★★
                // =========================================================
                if (mainCameraRig != null)
                {
                    mainCameraRig.position = defaultCameraPos;
                    mainCameraRig.rotation = defaultCameraRot;
                }
                // =========================================================
            }
            else
            {
                // === 情况 B: 参观好友家 (客人模式) ===
                buildingSystemObject.SetActive(false);
                if (returnHomeButton) returnHomeButton.SetActive(true);

                // 1. 主界面按钮状态
                if (mainBuildBtn) mainBuildBtn.SetActive(false);
                if (mainDiscoveryBtn) mainDiscoveryBtn.SetActive(false);
                if (mainVisitPreviewBtn) mainVisitPreviewBtn.SetActive(true);

                // 2. 预览界面内部按钮状态
                if (previewConfirmBtn) previewConfirmBtn.SetActive(false);
                if (previewExitBtn) previewExitBtn.SetActive(true);
            }
        }

        // --- 以下逻辑保持不变 ---

        foreach (Transform child in buildingRoot)
        {
            Destroy(child.gameObject);
        }

        LCQuery<LCUser> userQuery = LCUser.GetQuery();
        userQuery.WhereEqualTo("username", targetUsername);
        LCUser targetUser = await userQuery.First();

        if (targetUser == null) return;

        LCQuery<LCObject> buildQuery = new LCQuery<LCObject>("UserStructure");
        buildQuery.WhereEqualTo("owner", targetUser);
        var dataList = await buildQuery.Find();

        if (NPCManager.Instance != null)
        {
            NPCManager.Instance.ClearCounts();
        }

        // =========================================================
        // ★★★ 【关键修复】 增加了安全检查，防止崩溃！ ★★★
        // =========================================================
        foreach (var data in dataList)
        {
            // 1. 安全获取名字
            string name = data["prefabName"] as string;

            // 2. 如果名字是空的，直接跳过这一条，防止报错（ArgumentNullException）
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("⚠️ 发现一条坏数据（名字为空），已跳过。");
                continue;
            }

            // 3. 安全获取坐标
            float x = System.Convert.ToSingle(data["x"]);
            float z = System.Convert.ToSingle(data["z"]);
            float r = System.Convert.ToSingle(data["rotY"]); // 注意：检查这里是不是 rotY 还是 r，根据你上传的key

            // 过滤原点数据（可选）
            if (Mathf.Abs(x) < 0.01f && Mathf.Abs(z) < 0.01f)
            {
                // continue; // 根据需求决定是否过滤
            }

            // 4. 查字典
            if (buildingDict.ContainsKey(name))
            {
                GameObject prefab = buildingDict[name];

                if (prefab != null)
                {
                    // NPC 统计
                    var attr = prefab.GetComponent<BuildingAttribute>();
                    if (attr != null && NPCManager.Instance != null)
                    {
                        NPCManager.Instance.AddBuildingCount(attr.type);
                    }

                    // 生成
                    Vector3 pos = new Vector3(x, 0, z);
                    Quaternion rot = Quaternion.Euler(0, r, 0);
                    Instantiate(prefab, pos, rot, buildingRoot);
                }
            }
            else
            {
                // 如果字典里没有这个名字，打印错误但不崩溃
                Debug.LogError($"❌ 本地找不到建筑：[{name}]，请检查 Building List 是否包含此 Prefab。");
            }
        }
        // =========================================================

        Debug.Log("🏡 加载完毕，UI状态已更新。");
    }
}