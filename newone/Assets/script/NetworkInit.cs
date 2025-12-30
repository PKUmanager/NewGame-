using UnityEngine;
using LeanCloud;
using LeanCloud.Storage;

public class NetworkInit : MonoBehaviour
{
    // 去 LeanCloud 官网 -> 设置 -> 应用凭证 复制
    private string AppID = "Hb98xu7bCsfKEmdBLbe5jHGy-gzGzoHsz";
    private string AppKey = "UalCyp3k5TXJ1PZqT698Kc6c";
    private string ServerURL = "https://hb98xu7b.lc-cn-n1-shared.com"; // 比如 https://xxx.api.lncldglobal.com

    void Awake()
    {
        // 保证切换场景不销毁
        DontDestroyOnLoad(gameObject);

        // 初始化连接
        LCApplication.Initialize(AppID, AppKey, ServerURL);

        Debug.Log("✅ LeanCloud 初始化成功！");
    }
}