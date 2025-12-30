using UnityEngine;
using System.Collections;

public class RabbitDirectJump : MonoBehaviour
{
    [Header("跳跃参数")]
    public float jumpRadius = 3.0f;     // 跳跃范围
    public float minWait = 1.0f;        // 落地后蹲多久
    public float maxWait = 3.0f;
    public float jumpDuration = 0.8f;   // 【必须填对】动画文件的时长

    [Header("组件")]
    public Animator animator;           // 拖入子物体 Visual
    private Camera mainCam;

    private Vector3 startPos;           // 记录出生点

    void Start()
    {
        mainCam = Camera.main; // 获取主摄像机
        startPos = transform.position;
        StartCoroutine(JumpLoop());
    }

    IEnumerator JumpLoop()
    {
        while (true)
        {
            // 1. 等待（此时动画停在上一跳的最后一帧）
            float waitTime = Random.Range(minWait, maxWait);
            yield return new WaitForSeconds(waitTime);

            // 2. 计算目标
            Vector2 randomPoint = Random.insideUnitCircle * jumpRadius;
            Vector3 targetPos = startPos + new Vector3(randomPoint.x, 0, randomPoint.y);

            // 3. 判断方向并触发动画
            // 1. 算出移动的向量 (目标 - 当前)
            Vector3 moveDirection = targetPos - transform.position;

            // 2. 把这个世界方向，转换成“摄像机眼中的方向”
            // 这一步会自动把摄像机的旋转考虑进去
            Vector3 screenDir = mainCam.transform.InverseTransformDirection(moveDirection);

            // 3. 现在判断 screenDir.x > 0 就是真正的“屏幕右边”
            bool goRight = screenDir.x > 0;

            // 1. 先触发动画！
            if (goRight)
            {
                animator.SetTrigger("DoJumpRight");
            }
            else
            {
                animator.SetTrigger("DoJumpLeft");
            }

            // 2. 稍微等一帧（可选，为了防止动画还没切换就开始移，但这行通常不需要）
            // yield return null; 

            // 3. 然后再开始平移循环
            Vector3 currentPos = transform.position;
            float timer = 0f;

            while (timer < 1f)
            {
                // 让移动速度匹配动画播放速度
                timer += Time.deltaTime / jumpDuration;
                transform.position = Vector3.Lerp(currentPos, targetPos, timer);
                yield return null;
            }

            // 循环结束，回到步骤1继续等待
            // 此时动画播放完了，因为没勾Loop，它会自动定格在最后一帧不动
        }
    }
}