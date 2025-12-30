using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SocialWindowUI : MonoBehaviour
{
    [Header("Close")]
    [SerializeField] private Button btnClose;

    [Header("Friends List")]
    [SerializeField] private Transform friendsContent;      // FriendsList/Viewport/Content
    [SerializeField] private FriendRowUI friendRowPrefab;   // FriendRow Prefab（带脚本）
    [SerializeField] private Sprite defaultAvatar;
    // 假数据：后面换成你的真实好友数据/服务器数据
    private List<FriendData> friends = new List<FriendData>();

    private void Awake()
    {
        if (btnClose != null) btnClose.onClick.AddListener(() => gameObject.SetActive(false));
        BuildMockFriends();
    }

    private void OnEnable()
    {
        RefreshFriends();
    }

    private void RefreshFriends()
    {
        // 1. 清空旧内容
        for (int i = friendsContent.childCount - 1; i >= 0; i--)
        {
            Destroy(friendsContent.GetChild(i).gameObject);
        }

        // 2. 生成新内容（只要这一层 for）
        for (int i = 0; i < friends.Count; i++)
        {
            FriendData friend = friends[i];   // ? 只声明一次，且换个名字

            // 关键：worldPositionStays = false
            FriendRowUI row = Instantiate(friendRowPrefab, friendsContent, false);

            // 强制横向拉伸
            RectTransform rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, rt.anchorMin.y);
            rt.anchorMax = new Vector2(1f, rt.anchorMax.y);
            rt.offsetMin = new Vector2(0f, rt.offsetMin.y);
            rt.offsetMax = new Vector2(0f, rt.offsetMax.y);
            rt.localScale = Vector3.one;

            row.Bind(friend, OnClickVisitFriend);
        }

        // 3. 强制刷新布局
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(friendsContent as RectTransform);
    }
    private void OnClickVisitFriend(FriendData friend)
    {
        // 1?? 设置访问上下文
        VisitContext.Instance.VisitFriend(friend.playerId);

        // 2?? 切换到校园场景
        UnityEngine.SceneManagement.SceneManager.LoadScene("CampusScene");
    }

    private void BuildMockFriends()
    {
        friends.Clear();
        friends.Add(new FriendData("player_001", "好友A", defaultAvatar));
        friends.Add(new FriendData("player_002", "好友B", defaultAvatar));
    }
}

[System.Serializable]
public class FriendData
{
    public string playerId;
    public string name;
    public Sprite avatar;

    public FriendData(string playerId, string name, Sprite avatar)
    {
        this.playerId = playerId;
        this.name = name;
        this.avatar = avatar;
    }
}