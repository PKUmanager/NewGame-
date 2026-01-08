using UnityEngine;
using LeanCloud.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Collections;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using SysSave = SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem.SaveSystem;

public class HomeLoader : MonoBehaviour
{
    public static HomeLoader Instance;

    [Header("核心引用")]
    public Transform buildingRoot;
    public PlacementHandler placementHandler;
    public PlacementGrid placementGrid;
    public PlaceableObjectDatabase objectDatabase;

    [Header("UI 控制")]
    public GameObject mainBuildBtn;
    public GameObject mainVisitPreviewBtn;
    public GameObject mainDiscoveryBtn;
    public GameObject previewConfirmBtn;
    public GameObject previewExitBtn;
    public GameObject returnHomeButton;
    public GameObject buildingSystemObject;
    public TopBarUI_Legacy topBarUI;

    [Header("相机")]
    public Transform mainCameraRig;
    private Vector3 defaultCameraPos;
    private Quaternion defaultCameraRot;

    public List<GameObject> buildingList;
    private Dictionary<string, GameObject> buildingDict;

    void Awake()
    {
        Instance = this;
        InitDictionary();
        if (placementHandler == null) placementHandler = FindObjectOfType<PlacementHandler>();
        if (placementGrid == null) placementGrid = FindObjectOfType<PlacementGrid>();
        if (objectDatabase == null) objectDatabase = Resources.Load<PlaceableObjectDatabase>("PlaceableObjectsDatabase");
        if (objectDatabase == null && GameConfig.Instance != null) objectDatabase = GameConfig.Instance.PlaceableObjectDatabase;
        if (returnHomeButton != null) returnHomeButton.SetActive(false);
    }

    void InitDictionary()
    {
        buildingDict = new Dictionary<string, GameObject>();
        if (buildingList != null)
        {
            foreach (var prefab in buildingList)
                if (prefab != null && !buildingDict.ContainsKey(prefab.name))
                    buildingDict.Add(prefab.name, prefab);
        }
    }

    IEnumerator Start()
    {
        if (mainCameraRig != null)
        {
            defaultCameraPos = mainCameraRig.position;
            defaultCameraRot = mainCameraRig.rotation;
        }

        yield return new WaitForSeconds(0.5f);
        Task<LCUser> userTask = LCUser.GetCurrent();
        yield return new WaitUntil(() => userTask.IsCompleted);

        if (userTask.Result != null)
        {
            LoadHome(userTask.Result.Username);
        }
    }

    public void LoadHome(string targetUsername)
    {
        StopAllCoroutines();
        StartCoroutine(ProcessLoadHome(targetUsername));
    }

    private IEnumerator ProcessLoadHome(string targetUsername)
    {
        Debug.Log($"🚀 [HomeLoader] 准备加载: {targetUsername}");
        if (topBarUI != null) topBarUI.UpdateName(targetUsername);

        Task<LCUser> meTask = LCUser.GetCurrent();
        yield return new WaitUntil(() => meTask.IsCompleted);
        LCUser me = meTask.Result;
        bool isMyHome = (me != null && me.Username == targetUsername);

        // 1. 清理
        if (PlacementSystem.Instance != null) PlacementSystem.Instance.StopState();
        if (placementHandler != null) placementHandler.ClearEnvironment();
        else foreach (Transform child in buildingRoot) Destroy(child.gameObject);
        if (NPCManager.Instance != null) NPCManager.Instance.ClearCounts();

        yield return new WaitForEndOfFrame();
        yield return null;

        if (PlacementSystem.Instance != null && placementGrid != null)
        {
            PlacementSystem.Instance.Initialize(placementGrid);
        }

        // 2. 数据获取
        List<PlaceableObjectData> rawDataList = new List<PlaceableObjectData>();

        if (isMyHome)
        {
            SetupUIForOwner();
            ResetCamera();
            Debug.Log("🏠 读取本地缓存...");
            SaveData localData = SysSave.Load();

            if (localData != null && localData.placeableObjectDataCollection != null && localData.placeableObjectDataCollection.Count > 0)
            {
                Debug.Log($"✅ 使用本地数据 ({localData.placeableObjectDataCollection.Count})");
                rawDataList.AddRange(localData.placeableObjectDataCollection.Values);
            }
            else
            {
                Debug.LogWarning("⚠️ 本地为空，尝试云端恢复...");
                Task<List<PlaceableObjectData>> cloudTask = FetchCloudData(targetUsername);
                yield return new WaitUntil(() => cloudTask.IsCompleted);
                if (cloudTask.Result != null) rawDataList = cloudTask.Result;
            }
        }
        else
        {
            SetupUIForGuest();
            Task<List<PlaceableObjectData>> cloudTask = FetchCloudData(targetUsername);
            yield return new WaitUntil(() => cloudTask.IsCompleted);
            if (cloudTask.Result != null) rawDataList = cloudTask.Result;
        }

        // 3. 防重过滤
        List<PlaceableObjectData> finalCleanData = new List<PlaceableObjectData>();
        HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();

        foreach (var item in rawDataList)
        {
            if (occupiedPositions.Contains(item.gridPosition)) continue;
            occupiedPositions.Add(item.gridPosition);
            finalCleanData.Add(item);
        }

        // 4. 生成
        Debug.Log($"🏗️ 生成 {finalCleanData.Count} 个物体...");
        foreach (var podata in finalCleanData)
        {
            try
            {
                PlacementSystem.Instance.InitializeLoadedObject(podata);
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ 生成崩溃 [{podata.assetIdentifier}]: {e.Message}");
            }
        }
    }

    private async Task<List<PlaceableObjectData>> FetchCloudData(string targetUsername)
    {
        List<PlaceableObjectData> result = new List<PlaceableObjectData>();
        try
        {
            LCQuery<LCUser> userQuery = LCUser.GetQuery();
            userQuery.WhereEqualTo("username", targetUsername);
            LCUser targetUser = await userQuery.First();

            if (targetUser == null) return result;

            LCQuery<LCObject> buildQuery = new LCQuery<LCObject>("UserStructure");
            buildQuery.WhereEqualTo("owner", targetUser);
            buildQuery.Limit(1000);
            var cloudList = await buildQuery.Find();

            foreach (var data in cloudList)
            {
                string rawName = GetStringSafe(data, "prefabName");
                if (string.IsNullOrEmpty(rawName)) continue;
                string cleanName = rawName.Replace("(Clone)", "").Trim();

                // 获取基本信息
                float rot = GetFloatSafe(data, "rotY");

                // ★★★ 核心修复：优先读取 GridX/GridZ ★★★
                int? gridX = GetIntSafe(data, "gridX");
                int? gridZ = GetIntSafe(data, "gridZ");

                Vector3Int finalGridPos;

                if (gridX.HasValue && gridZ.HasValue)
                {
                    // 方案 A：如果有网格坐标（新存档），直接用！绝对准确！
                    finalGridPos = new Vector3Int(gridX.Value, 0, gridZ.Value);
                }
                else
                {
                    // 方案 B：旧存档（没有 GridX），只能靠算（可能会有偏移，但能兼容旧数据）
                    float x = GetFloatSafe(data, "posX");
                    float z = GetFloatSafe(data, "posZ");
                    Vector3 worldPosRaw = new Vector3(x, 0, z);
                    finalGridPos = placementGrid.WorldToCell(worldPosRaw);
                }

                Placeable placeable = FindPlaceableByPrefabName(cleanName);
                if (placeable != null)
                {
                    PlaceableObjectData pData = new PlaceableObjectData();
                    pData.assetIdentifier = placeable.GetAssetIdentifier();
                    pData.gridPosition = finalGridPos;
                    pData.direction = PlaceableUtils.GetDirection(Mathf.RoundToInt(rot));
                    pData.guid = System.Guid.NewGuid().ToString();

                    result.Add(pData);
                }
            }
        }
        catch (Exception e) { Debug.LogError("Cloud Fetch Error: " + e.Message); }
        return result;
    }

    // 辅助工具
    private Placeable FindPlaceableByPrefabName(string prefabName)
    {
        if (objectDatabase == null) return null;
        foreach (var p in objectDatabase.placeableObjects)
            if (p.Prefab != null && p.Prefab.name.Trim() == prefabName) return p;
        return null;
    }
    float GetFloatSafe(LCObject data, string key) { try { var val = data[key]; if (val != null) return Convert.ToSingle(val); } catch { } return 0f; }
    // 新增：读取整数
    int? GetIntSafe(LCObject data, string key) { try { var val = data[key]; if (val != null) return Convert.ToInt32(val); } catch { } return null; }
    string GetStringSafe(LCObject data, string key) { try { var val = data[key]; if (val != null) return val as string; } catch { } return null; }

    // UI 设置保持不变...
    void SetupUIForOwner() { buildingSystemObject.SetActive(true); if (returnHomeButton) returnHomeButton.SetActive(false); if (mainBuildBtn) mainBuildBtn.SetActive(true); if (mainDiscoveryBtn) mainDiscoveryBtn.SetActive(true); if (mainVisitPreviewBtn) mainVisitPreviewBtn.SetActive(false); if (previewConfirmBtn) previewConfirmBtn.SetActive(true); if (previewExitBtn) previewExitBtn.SetActive(false); }
    void SetupUIForGuest() { buildingSystemObject.SetActive(false); if (returnHomeButton) returnHomeButton.SetActive(true); if (mainBuildBtn) mainBuildBtn.SetActive(false); if (mainDiscoveryBtn) mainDiscoveryBtn.SetActive(false); if (mainVisitPreviewBtn) mainVisitPreviewBtn.SetActive(true); if (previewConfirmBtn) previewConfirmBtn.SetActive(false); if (previewExitBtn) previewExitBtn.SetActive(true); }
    void ResetCamera() { if (mainCameraRig != null) { mainCameraRig.position = defaultCameraPos; mainCameraRig.rotation = defaultCameraRot; } }
}