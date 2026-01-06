using UnityEngine;
using LeanCloud.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
// 引用核心命名空间
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
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

    // ★★★ 【新增】 模版账号名字 (用于新用户初始化) ★★★
    public string templateUserName = "YWJ";

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

        Debug.Log("🚀 正在前往 " + targetUsername + " 的家...");
        LCUser me = await LCUser.GetCurrent();

        // 1. 清空场景
        foreach (Transform child in buildingRoot) { Destroy(child.gameObject); }
        if (NPCManager.Instance != null) NPCManager.Instance.ClearCounts();

        // 2. UI 状态切换
        bool isMyHome = (me != null && me.Username == targetUsername);
        if (isMyHome)
        {
            SetupUIForOwner();
            ResetCamera();

            // =========================================================
            // ❌❌❌【修改点】 这一段必须注释掉！！！ ❌❌❌
            // 不要让它读本地文件了，否则永远不去云端！
            // =========================================================
            /*
            if (LoadLocalSave())
            {
                Debug.Log("✅ 本地存档加载成功，跳过云端同步。");
                return;
            }
            */
            // =========================================================
        }
        else
        {
            SetupUIForGuest();
        }

        // === 云端加载逻辑 ===

        LCQuery<LCUser> userQuery = LCUser.GetQuery();
        userQuery.WhereEqualTo("username", targetUsername);
        LCUser targetUser = await userQuery.First();

        if (targetUser == null)
        {
            Debug.LogError("查无此人");
            return;
        }

        LCQuery<LCObject> buildQuery = new LCQuery<LCObject>("UserStructure");
        buildQuery.WhereEqualTo("owner", targetUser);
        buildQuery.Limit(1000);
        var dataList = await buildQuery.Find();

        // =========================================================
        // ★★★ 【新增】 如果是空号，去加载模版数据！ ★★★
        // =========================================================
        if (dataList.Count == 0 && isMyHome)
        {
            Debug.LogWarning($"⚠️ 用户 [{targetUsername}] 的家是空的，尝试加载新手模版 [{templateUserName}]...");
            dataList = await GetTemplateData();
        }
        // =========================================================

        // 开始生成
        if (dataList != null)
        {
            foreach (var data in dataList)
            {
                string name = GetStringSafe(data, "prefabName");
                if (string.IsNullOrEmpty(name)) continue;

                float x = GetFloatSafe(data, "posX");
                float z = GetFloatSafe(data, "posZ");
                float r = GetFloatSafe(data, "rotY");

                // 兼容逻辑
                if (x == 0 && z == 0)
                {
                    float oldX = GetFloatSafe(data, "x");
                    float oldZ = GetFloatSafe(data, "z");
                    if (oldX != 0 || oldZ != 0) { x = oldX; z = oldZ; }
                }

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
        }

        // 最后刷新NPC
        if (NPCManager.Instance != null) NPCManager.Instance.CheckConditions();

        Debug.Log("☁️ 云端数据加载完毕。");
    }

    // ★★★ 【新增】 获取模版数据的方法 ★★★
    async Task<System.Collections.ObjectModel.ReadOnlyCollection<LCObject>> GetTemplateData()
    {
        LCQuery<LCUser> q = LCUser.GetQuery();
        q.WhereEqualTo("username", templateUserName);
        LCUser adminUser = await q.First();

        if (adminUser == null)
        {
            Debug.LogError($"❌ 模版账号 [{templateUserName}] 不存在！");
            return null;
        }

        LCQuery<LCObject> bq = new LCQuery<LCObject>("UserStructure");
        bq.WhereEqualTo("owner", adminUser);
        return await bq.Find();
    }

    // 工具方法
    float GetFloatSafe(LCObject data, string key)
    {
        try { var val = data[key]; if (val != null) return Convert.ToSingle(val); } catch { }
        return 0f;
    }
    string GetStringSafe(LCObject data, string key)
    {
        try { var val = data[key]; if (val != null) return val as string; } catch { }
        return null;
    }

    // (这个本地方法留着不删，但不调用它)
    private bool LoadLocalSave()
    {
        SaveData saveData = SaveSystem.Load();
        if (saveData == null || saveData.placeableObjectDataCollection.Count == 0) return false;
        var database = GameConfig.Instance.PlaceableObjectDatabase;
        foreach (var kvp in saveData.placeableObjectDataCollection)
        {
            PlaceableObjectData podata = kvp.Value;
            Placeable placeableObj = database.GetPlaceable(podata.assetIdentifier);
            if (placeableObj != null)
            {
                Vector3 worldPos = new Vector3(podata.gridPosition.x, 0, podata.gridPosition.z);
                placementHandler.PlaceLoadedObject(placeableObj, worldPos, podata, 1.0f);
            }
        }
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