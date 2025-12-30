using UnityEngine;
using System.Collections;

public class FourDirPathNPC : MonoBehaviour
{
    [Header("核心绑定 (必填)")]
    public Transform[] waypoints;   // 路径点
    public Animator animator;       // 子物体 Animator
    public Camera refCamera;        // 【新增】把你的 NPCCamera 拖进来！

    [Header("设置")]
    public float speed = 3.0f;
    public float waitTime = 2.0f;
    public bool loop = true;

    [Header("★★★ 角度修正滑块 ★★★")]
    [Range(-180, 180)]
    public float angleOffset = 0;   // 运行游戏时拖动这个来修正方向

    // 内部变量
    private int targetIndex = 0;
    private bool isWaiting = false;

    void Start()
    {
        // 自动防呆：如果你忘了拖相机，尝试自动找一下
        if (refCamera == null)
        {
            // 尝试找 tag 为 MainCamera 的
            refCamera = Camera.main;
            // 如果还没找到，尝试找名字叫 NPCCamera 的
            if (refCamera == null)
            {
                GameObject camObj = GameObject.Find("NPCCamera");
                if (camObj != null) refCamera = camObj.GetComponent<Camera>();
            }
        }

        if (waypoints.Length > 0) transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (waypoints.Length == 0 || isWaiting) return;

        // 1. 获取目标
        Transform target = waypoints[targetIndex];

        // 2. 计算世界移动向量
        Vector3 worldDir = target.position - transform.position;

        // 3. 判断到达
        if (worldDir.magnitude < 0.1f)
        {
            StartCoroutine(WaitNextPoint());
            animator.SetBool("IsMoving", false);
        }
        else
        {
            animator.SetBool("IsMoving", true);
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            // ====================================================
            // 4. 动画方向控制 (核心修正版)
            // ====================================================

            if (refCamera != null)
            {
                // A. 把世界方向转为“相机眼中的方向”
                // 这一步至关重要，它会处理掉你相机 -113.8 度的旋转
                Vector3 camDir = refCamera.transform.InverseTransformDirection(worldDir);

                // B. 加上你的手动修正 (Angle Offset)
                float rad = angleOffset * Mathf.Deg2Rad;
                float finalX = camDir.x * Mathf.Cos(rad) - camDir.z * Mathf.Sin(rad);
                float finalZ = camDir.x * Mathf.Sin(rad) + camDir.z * Mathf.Cos(rad);

                // C. 归一化并发送给 Animator
                Vector3 finalDir = new Vector3(finalX, 0, finalZ).normalized;
                animator.SetFloat("InputX", finalDir.x);
                animator.SetFloat("InputZ", finalDir.z);
            }
            else
            {
                Debug.LogError("脚本找不到相机！请在 Inspector 里把 NPCCamera 拖给 Ref Camera 变量！");
            }
        }
    }

    IEnumerator WaitNextPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        targetIndex++;
        if (targetIndex >= waypoints.Length)
        {
            if (loop) targetIndex = 0;
            else targetIndex = waypoints.Length - 1;
        }
        isWaiting = false;
    }
}