using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ToggleTargetByName : MonoBehaviour
{
    [Header("要开关的对象名字（名字必须完全一致）")]
    [SerializeField] private string targetObjectName = "Raby_Hat";

    private GameObject target;

    private void Awake()
    {
        // 先绑定按钮点击（不管目标有没有找到，按钮都必须能点）
        Button btn = GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("当前物体没有 Button 组件，请把脚本挂在真正的 Button 上");
            return;
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(Toggle);

        // 尝试找一次（找不到也没关系，点击时会再找）
        target = FindInSceneIncludingInactive(targetObjectName);
        if (target == null)
        {
            Debug.LogWarning($"Awake时未找到目标：{targetObjectName}（如果目标是隐藏的/稍后生成，点击时会再尝试查找）");
        }
    }

    private void Toggle()
    {
        // 如果之前没找到（或目标后来被销毁/重建），点击时再找一次
        if (target == null)
        {
            target = FindInSceneIncludingInactive(targetObjectName);
            if (target == null)
            {
                Debug.LogError($"点击时仍找不到目标对象：{targetObjectName}。请确认场景里确实有这个对象，并且名字完全一致。");
                return;
            }
        }

        target.SetActive(!target.activeSelf);
    }

    /// <summary>
    /// 在当前场景中查找指定名字的对象（包括 inactive 的对象）
    /// </summary>
    private GameObject FindInSceneIncludingInactive(string objName)
    {
        if (string.IsNullOrEmpty(objName)) return null;

        Scene scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            // true = 包括inactive子物体
            Transform[] all = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < all.Length; j++)
            {
                if (all[j].name == objName)
                    return all[j].gameObject;
            }
        }

        return null;
    }
}
