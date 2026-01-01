using UnityEngine;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    [Header("绑定 NPC 物体")]
    public GameObject npc_Rabbit; // 兔子
    public GameObject npc_YWJ;    // 工人 (铺地>=3)
    public GameObject npc_LMX;    // 居民 (家具>=3)
    public GameObject npc_YXK;    // 园丁 (景观>=3)
    public GameObject npc_YXL;    // 游客 (装饰>=3)
    public GameObject npc_WZF;    // 孩子 (四类都有)
    public GameObject npc_LDH;    // 大亨 (总数>=10)

    // 计数器字典
    private Dictionary<BuildingType, int> counts = new Dictionary<BuildingType, int>();

    void Awake()
    {
        Instance = this;
        ClearCounts(); // 初始化清零
    }

    // === 1. 清空计数（每次加载前调用）===
    public void ClearCounts()
    {
        counts[BuildingType.Terrains] = 0;
        counts[BuildingType.Facilities] = 0;
        counts[BuildingType.Nature] = 0;
        counts[BuildingType.Decorations] = 0;
        counts[BuildingType.None] = 0;
    }

    // === 2. 增加计数（建造/加载时调用）===
    public void AddBuildingCount(BuildingType type)
    {
        if (counts.ContainsKey(type))
        {
            counts[type]++;
        }
        // 每次增加后，立刻检查是否触发 NPC
        CheckConditions();
    }

    // === 3. 核心：检查条件并开关 NPC ===
    public void CheckConditions()
    {
        // 获取当前数量
        int terrainCount = counts[BuildingType.Terrains];
        int facilityCount = counts[BuildingType.Facilities];
        int natureCount = counts[BuildingType.Nature];
        int decoCount = counts[BuildingType.Decorations];
        int totalCount = terrainCount + facilityCount + natureCount + decoCount;

        // --- 兔子 (无条件) ---
        if (npc_Rabbit) npc_Rabbit.SetActive(true);

        // --- NPC 1: YWJ (铺地 >= 3) ---
        if (npc_YWJ) npc_YWJ.SetActive(terrainCount >= 3);

        // --- NPC 2: LMX (家具 >= 3) ---
        if (npc_LMX) npc_LMX.SetActive(facilityCount >= 3);

        // --- NPC 3: YXK (景观 >= 3) ---
        if (npc_YXK) npc_YXK.SetActive(natureCount >= 3);

        // --- NPC 4: YXL (装饰 >= 3) ---
        if (npc_YXL) npc_YXL.SetActive(decoCount >= 3);

        // --- NPC 5: WZF (四类都出现) ---
        bool allTypesPresent = (terrainCount > 0) && (facilityCount > 0) && (natureCount > 0) && (decoCount > 0);
        if (npc_WZF) npc_WZF.SetActive(allTypesPresent);

        // --- NPC 6: LDH (总和 >= 10) ---
        if (npc_LDH) npc_LDH.SetActive(totalCount >= 10);

        // 调试日志（方便你看数对了没）
        Debug.Log($"繁荣度统计: 地{terrainCount} 家具{facilityCount} 景{natureCount} 饰{decoCount} | 总{totalCount}");
    }
}