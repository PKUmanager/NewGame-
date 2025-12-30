using UnityEngine;

public class CampusSceneManager : MonoBehaviour
{
    private void Start()
    {
        if (VisitContext.Instance == null)
        {
            Debug.LogError("VisitContext 不存在");
            return;
        }

        if (VisitContext.Instance.Mode == CampusVisitMode.Self)
        {
            LoadMyCampus(VisitContext.Instance.TargetPlayerId);
        }
        else
        {
            LoadFriendCampus(VisitContext.Instance.TargetPlayerId);
        }
    }

    private void LoadMyCampus(string myId)
    {
        // 自己校园：可建造
        // 1. 读取本地存档 / 本地数据
        // 2. 生成校园
        Debug.Log("加载自己的校园：" + myId);
    }

    private void LoadFriendCampus(string friendId)
    {
        // 好友校园：只读
        // 1. 从服务器 / 模拟数据读取好友校园
        // 2. 生成校园
        Debug.Log("访问好友校园：" + friendId);
    }
}