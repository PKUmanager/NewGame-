using UnityEngine;
using SpaceFusion.SF_Grid_Building_System.Scripts.Managers;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;

/// <summary>
/// 开发专用工具：按下 F5 将当前场景保存为“游戏默认初始场景”
/// </summary>
public class LevelDevTool : MonoBehaviour
{
#if UNITY_EDITOR
    void Update()
    {
        // 按下 F5 键触发
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveCurrentAsDefault();
        }
    }

    void SaveCurrentAsDefault()
    {
        // 获取当前的 saveData
        if (GameManager.Instance != null && GameManager.Instance.saveData != null)
        {
            SaveSystem.SaveAsDefaultLevel(GameManager.Instance.saveData);
        }
        else
        {
            Debug.LogError("保存失败：无法获取 GameManager 或 saveData！请先运行游戏并放置一些物品。");
        }
    }
#endif
}