using UnityEngine;
using System.Collections;
using UnityEngine.UI; // 如果要用气泡对话

public class NPCWalker : MonoBehaviour
{
    [Header("设置路径点")]
    public Transform[] waypoints; // 把你的 Path_A, Path_B 拖进来

    [Header("移动设置")]
    public float speed = 2.0f;
    public float waitTime = 1.0f; // 到达后停留时间

    [Header("对话气泡")]
    public GameObject speechBubble; // 拖入一个UI Text或者图片做气泡
    public Text speechText;

    private int currentPointIndex = 0;
    private bool isMoving = true;

    void Start()
    {
        // 一开始隐藏气泡
        if (speechBubble) speechBubble.SetActive(false);

        // 即使没有设置路径，防止报错
        if (waypoints.Length > 0)
        {
            // 把NPC瞬移到第一个点作为起点
            transform.position = waypoints[0].position;
        }
    }

    void Update()
    {
        if (!isMoving || waypoints.Length == 0) return;

        // 1. 获取当前目标点
        Transform target = waypoints[currentPointIndex];

        // 2. 向目标移动
        // 使用 Vector3.MoveTowards 匀速移动
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // 3. 判断是否到达 (距离极小)
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            // 到达了某个点
            StartCoroutine(OnReachPoint());
        }
    }

    IEnumerator OnReachPoint()
    {
        isMoving = false; // 暂停移动

        // 如果是终点 (最后一个点)
        if (currentPointIndex == waypoints.Length - 1)
        {
            // === 这里处理说话逻辑 ===
            ShowSpeech("这个建筑盖得真不错！\n特别是那个红色的屋顶！");

            // 如果你想让NPC说完话消失，可以加：
            // yield return new WaitForSeconds(3f);
            // Destroy(gameObject);
        }
        else
        {
            // 如果不是终点，休息一会儿继续走
            yield return new WaitForSeconds(waitTime);
            currentPointIndex++; // 切换到下一个点
            isMoving = true; // 继续走


        }
    }

    void ShowSpeech(string content)
    {
        if (speechBubble)
        {
            speechBubble.SetActive(true);
            if (speechText) speechText.text = content;
        }
    }
}