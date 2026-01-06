using UnityEngine;
using UnityEngine.UI;

public class ToggleTargetByName : MonoBehaviour
{
    [Header("场景里要开关的对象名字（必须完全一致）")]
    [SerializeField] private string targetObjectName = "Raby_Hat";

    private GameObject target;

    private void Awake()
    {
        // 找到目标
        target = GameObject.Find(targetObjectName);

        if (target == null)
        {
            Debug.LogError($"找不到目标对象：{targetObjectName}，请检查场景里是否存在并且名字完全一致");
            return;
        }

        // 给本按钮绑定点击事件（无需在Inspector里配OnClick）
        Button btn = GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("当前物体没有 Button 组件，请把脚本挂在真正的 Button 上");
            return;
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(Toggle);
    }

    private void Toggle()
    {
        if (target != null)
            target.SetActive(!target.activeSelf);
    }
}