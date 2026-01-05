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
    // ★★★ 1. 新增：发现按钮
    public GameObject mainDiscoveryBtn;    // Discovery_Btn (发现)

    [Header("预览界面内部 UI 控制")]
    public GameObject previewConfirmBtn;   // 开始建造/确定 (主人用)
    // ★★★ 2. 新增：退出预览按钮
    public GameObject previewExitBtn;      // 退出预览 (客人用)

    public List<GameObject> buildingList;
    private Dictionary<string, GameObject> buildingDict;

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
        await System.Threading.Tasks.Task.Delay(500);
        LCUser currentUser = await LCUser.GetCurrent();

        if (currentUser != null)
        {
            LoadHome(currentUser.Username);
        }
    }

    public async void LoadHome(string targetUsername)
    {
        LCUser me = await LCUser.GetCurrent();

        if (me != null && buildingSystemObject != null)
        {
            if (me.Username == targetUsername)
            {
                // === 情况 A: 回到自己家 (主人模式) ===
                buildingSystemObject.SetActive(true);
                if (returnHomeButton) returnHomeButton.SetActive(false);

                // 1. 主界面按钮状态
                if (mainBuildBtn) mainBuildBtn.SetActive(true);             // 显示【建造】
                if (mainDiscoveryBtn) mainDiscoveryBtn.SetActive(true);     // 显示【发现】
                if (mainVisitPreviewBtn) mainVisitPreviewBtn.SetActive(false); // 隐藏【预览入口】(主人不需要单纯的预览)

                // 2. 预览界面内部按钮状态
                if (previewConfirmBtn) previewConfirmBtn.SetActive(true);   // 显示【开始建造】
                if (previewExitBtn) previewExitBtn.SetActive(false);        // 隐藏【退出预览】(假设主人用其他方式或不需要这个特定按钮)
            }
            else
            {
                // === 情况 B: 参观好友家 (客人模式) ===
                buildingSystemObject.SetActive(false);
                if (returnHomeButton) returnHomeButton.SetActive(true);

                // 1. 主界面按钮状态
                if (mainBuildBtn) mainBuildBtn.SetActive(false);            // 销毁/隐藏【建造】
                if (mainDiscoveryBtn) mainDiscoveryBtn.SetActive(false);    // 销毁/隐藏【发现】
                if (mainVisitPreviewBtn) mainVisitPreviewBtn.SetActive(true);  // 启用【预览入口】

                // 2. 预览界面内部按钮状态
                // 这样当你点开预览界面时，"开始建造"是灭的，"退出预览"是亮的
                if (previewConfirmBtn) previewConfirmBtn.SetActive(false);  // 销毁/隐藏【开始建造】
                if (previewExitBtn) previewExitBtn.SetActive(true);         // 启用【退出预览】
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

        foreach (var data in dataList)
        {
            string name = data["prefabName"] as string;
            float x = System.Convert.ToSingle(data["x"]);
            float z = System.Convert.ToSingle(data["z"]);
            float r = System.Convert.ToSingle(data["r"]);

            if (Mathf.Abs(x) < 0.01f && Mathf.Abs(z) < 0.01f)
            {
                continue;
            }

            GameObject prefab = null;
            if (buildingDict.ContainsKey(name))
            {
                prefab = buildingDict[name];
            }

            if (prefab != null)
            {
                var attr = prefab.GetComponent<BuildingAttribute>();
                if (attr != null && NPCManager.Instance != null)
                {
                    NPCManager.Instance.AddBuildingCount(attr.type);
                }

                Vector3 pos = new Vector3(x, 0, z);
                Quaternion rot = Quaternion.Euler(0, r, 0);
                Instantiate(prefab, pos, rot, buildingRoot);
            }
        }
        Debug.Log("🏡 加载完毕，UI状态已更新。");
    }
}