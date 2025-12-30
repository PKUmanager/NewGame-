using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class NPCController : MonoBehaviour
{
    [Header("移动设置")]
    public float wanderRadius = 10f; // 游荡范围半径
    public float minWaitTime = 2f;   // 站立最少停多久
    public float maxWaitTime = 5f;   // 站立最多停多久

    [Header("绑定组件")]
    public Animator animator;        // 拖入子物体 Visual 上的 Animator
    public GameObject speechBubble;  // 拖入头顶的气泡物体
    public Text speechText;          // 拖入气泡里的文字

    [Header("对话内容库")]
    public string[] randomDialogs = {
        "奥卡姆剃刀原则，好！",
        "好想去搬砖啊",
        "这个设计，看得我眼睛都湿了",
        "太棒啦！"
    };

    private NavMeshAgent agent;
    private float waitTimer;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 防止父物体乱转，我们只希望父物体移动坐标
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // 一开始先隐藏气泡
        if (speechBubble) speechBubble.SetActive(false);

        // 立即开始找一个点走
        MoveToRandomPoint();
    }

    void Update()
    {
        // === 1. 处理动画参数 (核心) ===
        // 获取 AI 的速度向量
        Vector3 vel = agent.velocity;

        // 如果速度很小，就是停下了
        bool isMoving = vel.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            // 把速度归一化（变成 -1 到 1 之间），传给混合树
            // vel.x 对应 DirX, vel.z 对应 DirZ
            animator.SetFloat("DirX", vel.normalized.x);
            animator.SetFloat("DirZ", vel.normalized.z);

            // 确保气泡隐藏（走路时不说话）
            if (speechBubble.activeSelf) speechBubble.SetActive(false);
        }

        // === 2. 游荡逻辑 ===
        // 如果正在等待状态，倒计时
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                // 时间到了，开始走
                isWaiting = false;
                MoveToRandomPoint();
            }
            return;
        }

        // 如果没有在该等待，且距离目标很近了 -> 到达目的地
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(OnReachDestination());
        }
    }

    // 到达目的地后的处理
    IEnumerator OnReachDestination()
    {
        isWaiting = true; // 进入等待状态

        // 随机停顿几秒
        waitTimer = Random.Range(minWaitTime, maxWaitTime);

        // === 弹出对话 ===
        if (speechBubble && speechText)
        {
            speechBubble.SetActive(true);
            // 随机选一句话
            string talk = randomDialogs[Random.Range(0, randomDialogs.Length)];
            speechText.text = talk;
        }

        // 这里不需要 yield return，因为我们在 Update 里用 waitTimer 计时了
        yield return null;
    }

    // 找随机点
    void MoveToRandomPoint()
    {
        Vector3 randomPoint = GetRandomPoint(transform.position, wanderRadius, -1);
        agent.SetDestination(randomPoint);
    }

    // NavMesh 随机点算法
    Vector3 GetRandomPoint(Vector3 center, float range, int areaMask)
    {
        Vector3 randomPos = Random.insideUnitSphere * range;
        randomPos += center;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, range, areaMask);
        return hit.position;
    }
}