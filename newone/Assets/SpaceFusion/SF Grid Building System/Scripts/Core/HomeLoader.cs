using UnityEngine;
using LeanCloud.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
// 引用核心命名空间
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
// ★★★ 【修复 CS0103】 必须引用 Utils 才能找到 GameConfig ★★★
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;

public class HomeLoader : MonoBehaviour
{
    public static HomeLoader Instance;

    [Header("建筑生成的根节点")]
    public Transform buildingRoot;
    public GameObject returnHomeButton;

    public GameObject buildingSystemObject;

    [Header("UI 控制")]
    public GameObject mainBuildBtn;
    public GameObject mainVisitPreviewBtn;
    public GameObject mainDiscoveryBtn;
    public GameObject previewConfirmBtn;
    public GameObject previewExitBtn;

    [Header("相机控制")]
    public Transform mainCameraRig;
    private Vector3 defaultCameraPos;
    private Quaternion defaultCameraRot;

    // 引用 PlacementHandler 用于本地生成
    public PlacementHandler placementHandler;

    public List<GameObject> buildingList;
    private Dictionary<string, GameObject> buildingDict;

    public TopBarUI_Legacy topBarUI;

    void Awake()
    {
        Instance = this;
        InitDictionary();
        if (returnHomeButton != null) returnHomeButton.SetActive(false);
        if (placementHandler == null) placementHandler = FindObjectOfType<PlacementHandler>();
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
        if (mainCameraRig != null)
        {
            defaultCameraPos = mainCameraRig.position;
            defaultCameraRot = mainCameraRig.rotation;
        }

        await System.Threading.Tasks.Task.Delay(500);
        LCUser currentUser = await LCUser.GetCurrent();

        if (currentUser != null)
        {
            LoadHome(currentUser.Username);
        }
    }

    public async void LoadHome(string targetUsername)
    {
        if (topBarUI != null) topBarUI.UpdateName(targetUsername);

        Debug.Log("正在前往 " + targetUsername + " 的家...");
        LCUser me = await LCUser.GetCurrent();

        // 1. 清空场景
        foreach (Transform child in buildingRoot) { Destroy(child.gameObject); }
        if (NPCManager.Instance != null) NPCManager.Instance.ClearCounts();

        if (me != null)
        {
            if (me.Username == targetUsername)
            {
                // === 回到自己家 ===
                SetupUIForOwner();
                ResetCamera();

                // ★★★ 优先尝试加载本地存档 ★★★
                Debug.Log("🏠 正在加载本地存档...");
                // 如果本地加载成功，直接返回，不再请求云端
                if (LoadLocalSave())
                {
                    Debug.Log("✅ 本地存档加载成功，跳过云端同步。");
                    return;
                }

                Debug.Log("⚠️ 本地存档为空，尝试从云端拉取备份...");
            }
            else
            {
                // === 去别人家 ===
                SetupUIForGuest();
            }
        }

        // === 云端加载逻辑 ===

        LCQuery<LCUser> userQuery = LCUser.GetQuery();
        userQuery.WhereEqualTo("username", targetUsername);
        LCUser targetUser = await userQuery.First();

        if (targetUser == null) return;

        LCQuery<LCObject> buildQuery = new LCQuery<LCObject>("UserStructure");
        buildQuery.WhereEqualTo("owner", targetUser);
        buildQuery.Limit(1000); // 确保拉取所有建筑
        var dataList = await buildQuery.Find();

        foreach (var data in dataList)
        {
            // 使用安全方法获取名字
            string name = GetStringSafe(data, "prefabName");
            if (string.IsNullOrEmpty(name)) continue;

            // ★★★ 兼容新旧数据，防止 (0,0,0) 堆叠 ★★★
            // 1. 尝试读新 Key (posX)
            float x = GetFloatSafe(data, "posX");
            float z = GetFloatSafe(data, "posZ");
            float r = GetFloatSafe(data, "rotY");

            // 2. 如果新 Key 没数据，尝试读旧 Key (x)
            if (x == 0 && z == 0)
            {
                float oldX = GetFloatSafe(data, "x");
                float oldZ = GetFloatSafe(data, "z");
                if (oldX != 0 || oldZ != 0)
                {
                    x = oldX;
                    z = oldZ;
                    // 尝试补救旋转
                    float oldR = GetFloatSafe(data, "r");
                    if (r == 0 && oldR != 0) r = oldR;
                }
            }

            // 3. 过滤无效原点数据
            if (Mathf.Abs(x) < 0.001f && Mathf.Abs(z) < 0.001f) continue;

            if (buildingDict.ContainsKey(name))
            {
                GameObject prefab = buildingDict[name];
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
        }
        Debug.Log("☁️ 云端数据加载完毕。");
    }

    // ★★★ [工具] 安全读取 float (防止 Keys 报错) ★★★
    float GetFloatSafe(LCObject data, string key)
    {
        try
        {
            var val = data[key];
            if (val != null) return Convert.ToSingle(val);
        }
        catch { }
        return 0f;
    }

    // ★★★ [工具] 安全读取 string ★★★
    string GetStringSafe(LCObject data, string key)
    {
        try
        {
            var val = data[key];
            if (val != null) return val as string;
        }
        catch { }
        return null;
    }

    // ★★★ 加载本地存档 (逻辑已修正为匹配你的数据库) ★★★
    private bool LoadLocalSave()
    {
        SaveData saveData = SaveSystem.Load();
        if (saveData == null || saveData.placeableObjectDataCollection.Count == 0) return false;

        // 获取数据库引用
        var database = GameConfig.Instance.PlaceableObjectDatabase;

        foreach (var kvp in saveData.placeableObjectDataCollection)
        {
            PlaceableObjectData podata = kvp.Value;

            // ★★★ 【修正】 使用 assetIdentifier 而不是 ID ★★★
            // 因为你提供的 PlaceableObjectDatabase.cs 里只有 GetPlaceable(string)，没有 GetItem(int)
            // assetIdentifier 是 Data 基类自带的，这样读取绝对安全
            Placeable placeableObj = database.GetPlaceable(podata.assetIdentifier);

            if (placeableObj != null)
            {
                Vector3 worldPos = new Vector3(podata.gridPosition.x, 0, podata.gridPosition.z);
                placementHandler.PlaceLoadedObject(placeableObj, worldPos, podata, 1.0f);
            }
            else
            {
                Debug.LogWarning($"本地存档中的物品 [{podata.assetIdentifier}] 在数据库中未找到。");
            }
        }
        Debug.Log($"💾 本地存档加载成功: {saveData.placeableObjectDataCollection.Count} 个建筑");
        return true;
    }

    void SetupUIForOwner()
    {
        buildingSystemObject.SetActive(true);
        if (returnHomeButton) returnHomeButton.SetActive(false);
        if (mainBuildBtn) mainBuildBtn.SetActive(true);
        if (mainDiscoveryBtn) mainDiscoveryBtn.SetActive(true);
        if (mainVisitPreviewBtn) mainVisitPreviewBtn.SetActive(false);
        if (previewConfirmBtn) previewConfirmBtn.SetActive(true);
        if (previewExitBtn) previewExitBtn.SetActive(false);
    }

    void SetupUIForGuest()
    {
        buildingSystemObject.SetActive(false);
        if (returnHomeButton) returnHomeButton.SetActive(true);
        if (mainBuildBtn) mainBuildBtn.SetActive(false);
        if (mainDiscoveryBtn) mainDiscoveryBtn.SetActive(false);
        if (mainVisitPreviewBtn) mainVisitPreviewBtn.SetActive(true);
        if (previewConfirmBtn) previewConfirmBtn.SetActive(false);
        if (previewExitBtn) previewExitBtn.SetActive(true);
    }

    void ResetCamera()
    {
        if (mainCameraRig != null)
        {
            mainCameraRig.position = defaultCameraPos;
            mainCameraRig.rotation = defaultCameraRot;
        }
    }
}