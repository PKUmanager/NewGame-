using UnityEngine;

[System.Serializable]
public class UserData
{
    public string username;     // 用户名
    public string password;     // 密码（在本地存储时需要加密处理）
    public string campusName;   // 校园名称
    public float vitality;      // 校园活力值
    public string avatar;       // 头像路径（如果有头像的话）
    public bool isFirstLogin;   // 是否首次登录（可以用来判断是否需要引导）

    // 其他校园信息可以根据需要添加，例如：
    // public List<Building> buildings;  // 存储建筑物等
    // public List<Item> inventory;      // 存储背包物品等

    // 构造函数
    public UserData(string username, string password, string campusName)
    {
        this.username = username;
        this.password = password;
        this.campusName = campusName;
        this.vitality = 100f;  // 默认活力值为100
        this.avatar = "defaultAvatar";  // 默认头像
        this.isFirstLogin = true;  // 默认首次登录
    }
}
