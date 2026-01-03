using UnityEngine;
using LeanCloud;
using LeanCloud.Storage;
using UnityEngine.SceneManagement;

public class LogoutManager : MonoBehaviour
{
    public void OnLogoutClick()
    {
        // 1. 清除本地的钥匙 (注意是 Logout 不是 LogOut)
        LCUser.Logout();

        Debug.Log("已注销！");

        // 2. 踢回登录场景
        // (请确保 Build Settings 里有这个场景，且名字完全一致)
        SceneManager.LoadScene("StartupScene");
    }
}