using UnityEngine;

public class DiscoveryUIHandler : MonoBehaviour
{
    [Header("把 FunctionButtons 拖到这里")]
    public GameObject functionButtons;

    // 这个函数专门给按钮点击用
    public void OnClickDiscover()
    {
        if (functionButtons != null)
        {
            functionButtons.SetActive(false); // 隐藏物体
            Debug.Log("FunctionButtons 已隐藏！");
        }
        else
        {
            Debug.LogError("你忘记在 Inspector 里拖拽 FunctionButtons 了！");
        }
    }
}