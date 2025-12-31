using UnityEngine;
using TMPro; // 如果用的是旧版Text，就把 TMP_Text 改成 Text

public class FriendItem : MonoBehaviour
{
    public TMP_Text nameText; // 拖入显示名字的 Text
    private string myFriendName;

    // 初始化函数：给 SocialManager 调用的
    public void Setup(string name)
    {
        myFriendName = name;
        nameText.text = name;
    }

    // 绑定到“拜访”按钮
    public void OnVisitClick()
    {
        // 核心：调用之前的 HomeLoader 去下载他的家
        HomeLoader.Instance.LoadHome(myFriendName);

        // (可选) 拜访后自动关闭好友窗口
        // transform.root.GetComponentInChildren<SocialManager>().gameObject.SetActive(false);
    }
}