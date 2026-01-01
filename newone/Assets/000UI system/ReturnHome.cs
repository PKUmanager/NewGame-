using UnityEngine;
using LeanCloud.Storage;

public class ReturnHome : MonoBehaviour
{
    // 绑定到按钮的 OnClick
    public async void OnClickGoHome()
    {
        // 1. 获取我是谁
        LCUser me = await LCUser.GetCurrent();

        if (me != null)
        {
            Debug.Log("正在返回 " + me.Username + " 的校园...");

            // 2. 调用 HomeLoader 加载我的名字
            // (这一步跟拜访好友是一模一样的逻辑，只是参数换成了自己)
            HomeLoader.Instance.LoadHome(me.Username);
        }
        else
        {
            Debug.LogError("还没登录，这就尴尬了...");
        }
    }
}