using UnityEngine;
using TMPro; // 如果用旧版UI，引用 UnityEngine.UI
using LeanCloud.Storage;
using System.Collections.Generic;

public class SocialManager : MonoBehaviour
{
    [Header("把刚才补做的UI拖进去")]
    public TMP_InputField searchInput; // 搜索框
    public GameObject friendItemPrefab; // 那个做好的Prefab

    [Header("把现有的 Content 拖进去")]
    public Transform listContent; // FriendList -> Viewport -> Content

    // 窗口打开时，自动刷新列表
    void OnEnable()
    {
        RefreshList();
    }

    // === 功能：添加好友 ===
    public async void OnAddBtnClick()
    {
        string name = searchInput.text;
        if (string.IsNullOrEmpty(name)) return;

        Debug.Log("查找用户：" + name);

        // 1. 查人
        LCQuery<LCUser> userQuery = LCUser.GetQuery();
        userQuery.WhereEqualTo("username", name);
        LCUser targetUser = await userQuery.First();

        if (targetUser == null)
        {
            Debug.Log("查无此人");
            return;
        }

        // 2. 存关系 (Friendship 表)
        LCUser me = await LCUser.GetCurrent();
        if (me.ObjectId == targetUser.ObjectId) return; // 不能加自己

        LCObject friendship = new LCObject("Friendship");
        friendship["user"] = me;
        friendship["friend"] = targetUser;

        await friendship.Save();
        Debug.Log("添加成功");

        searchInput.text = ""; // 清空输入框
        RefreshList(); // 刷新显示
    }

    // === 功能：刷新列表 ===
    public async void RefreshList()
    {
        // 1. 清空旧的 UI
        foreach (Transform child in listContent) Destroy(child.gameObject);

        // 2. 查我的好友
        LCUser me = await LCUser.GetCurrent();
        if (me == null) return;

        LCQuery<LCObject> query = new LCQuery<LCObject>("Friendship");
        query.WhereEqualTo("user", me);
        query.Include("friend"); // 把朋友的详细信息也拉回来

        var results = await query.Find();

        // 3. 生成新的 UI
        foreach (var item in results)
        {
            LCUser friend = item["friend"] as LCUser;
            if (friend != null)
            {
                // 生成一条
                GameObject newItem = Instantiate(friendItemPrefab, listContent);
                // 填名字
                newItem.GetComponent<FriendItem>().Setup(friend.Username);
            }
        }
    }

    // === 绑定到 Btn_Close ===
    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}