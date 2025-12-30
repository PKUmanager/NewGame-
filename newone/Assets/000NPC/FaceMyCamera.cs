using UnityEngine;

public class FaceMyCamera : MonoBehaviour
{
    [Header("把你的 NPCCamera 拖进来")]
    public Camera targetCamera;

    void Start()
    {
        // 如果忘了拖，尝试自动找一下名字叫 NPCCamera 的物体
        if (targetCamera == null)
        {
            GameObject camObj = GameObject.Find("NPCCamera");
            if (camObj != null) targetCamera = camObj.GetComponent<Camera>();
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        // 【核心】直接复制摄像机的旋转角度
        // 这样图片平面就永远和摄像机镜头平行
        transform.rotation = targetCamera.transform.rotation;
    }
}