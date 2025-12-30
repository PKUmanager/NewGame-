using UnityEngine;

public class UserManager : MonoBehaviour
{
    private const string USER_DATA_KEY = "UserData"; // 用于存储的键值
    private UserData currentUser;

    private void Start()
    {
        // 在游戏启动时加载用户信息
        LoadUserData();
    }

    // 加载用户信息（从 PlayerPrefs 或文件）
    public void LoadUserData()
    {
        if (PlayerPrefs.HasKey(USER_DATA_KEY))
        {
            // 从 PlayerPrefs 加载
            string json = PlayerPrefs.GetString(USER_DATA_KEY);
            currentUser = JsonUtility.FromJson<UserData>(json);
        }
        else
        {
            // 如果没有保存的数据，创建默认的用户数据
            currentUser = new UserData("DefaultUser", "1234", "My Campus");
        }
    }

    // 保存用户信息（使用 PlayerPrefs 或文件）
    public void SaveUserData()
    {
        string json = JsonUtility.ToJson(currentUser);
        PlayerPrefs.SetString(USER_DATA_KEY, json);  // 保存到 PlayerPrefs
        PlayerPrefs.Save();
    }

    // 设置用户信息（如用户名、校园名称）
    public void SetUserInfo(string username, string campusName)
    {
        currentUser.username = username;
        currentUser.campusName = campusName;
        SaveUserData();  // 每次更改后保存
    }

    // 获取当前用户数据
    public UserData GetCurrentUser()
    {
        return currentUser;
    }

    // 检查是否是首次登录
    public bool IsFirstLogin()
    {
        return currentUser.isFirstLogin;
    }

    // 设置用户为非首次登录
    public void SetFirstLoginFalse()
    {
        currentUser.isFirstLogin = false;
        SaveUserData();  // 修改后保存
    }
}