using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class DeepSeekDialogueManager : MonoBehaviour
{
    // API配置
    [Header("API Settings")]
    [SerializeField] private string apiKey = "sk-9822dd240d204cfe87ccb4848c71640b"; // DeepSeek API密钥
    [SerializeField] private string modelName = "deepseek-chat"; // 使用的模型名称
    [SerializeField] private string apiUrl = "https://api.deepseek.com/v1/chat/completions"; // API请求地址

    // 对话参数
    [Header("Dialogue Settings")]
    [Range(0, 2)] public float temperature = 0.7f; // 控制生成文本的随机性（0-2，值越高越随机）
    [Range(1, 1000)] public int maxTokens = 150; // 生成的最大令牌数（控制回复长度）

    // 角色设定
    [System.Serializable]
    public class NPCCharacter
    {
        public string name;
        [TextArea(3, 10)]
        public string personalityPrompt = "你是虚拟人物草兔，是个性格活泼，乐观自由的小兔子。擅长审美和设计知识。";
    }
    [SerializeField] public NPCCharacter npcCharacter;

    // 回调委托，用于异步处理API响应
    public delegate void DialogueCallback(string response, bool isSuccess);

    /// <summary>
    /// 发送对话请求
    /// </summary>
    /// <param name="userMessage">玩家输入内容</param>
    /// <param name="callback">处理API响应的回调函数</param>
    public void SendDialogueRequest(string userMessage, DialogueCallback callback)
    {
        StartCoroutine(ProcessDialogueRequest(userMessage, callback));
    }

    private IEnumerator ProcessDialogueRequest(string userInput, DialogueCallback callback)
    {
        // 构建消息列表
        List<Message> messages = new List<Message> {
            new Message { role = "system", content = npcCharacter.personalityPrompt },
            new Message { role = "user", content = userInput }
        };

        // 构建请求体
        ChatRequest requestBody = new ChatRequest
        {
            model = modelName,
            messages = messages,
            temperature = temperature,
            max_tokens = maxTokens
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log("Sending JSON: " + jsonBody);

        UnityWebRequest request = CreateWebRequest(jsonBody);
        yield return request.SendWebRequest();

        if (IsRequestError(request))
        {
            Debug.LogError($"API Error: {request.responseCode}\n{request.downloadHandler.text}");
            callback?.Invoke(null, false);
            yield break;
        }

        DeepSeekResponse response = ParseResponse(request.downloadHandler.text);
        if (response != null && response.choices.Length > 0)
        {
            string npcReply = response.choices[0].message.content;
            callback?.Invoke(npcReply, true);
        }
        else
        {
            callback?.Invoke(name + "（陷入沉默）", false);
        }
    }

    private UnityWebRequest CreateWebRequest(string jsonBody)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        var request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Accept", "application/json");
        return request;
    }

    private bool IsRequestError(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.ConnectionError
            || request.result == UnityWebRequest.Result.ProtocolError
            || request.result == UnityWebRequest.Result.DataProcessingError;
    }

    private DeepSeekResponse ParseResponse(string jsonResponse)
    {
        try
        {
            return JsonUtility.FromJson<DeepSeekResponse>(jsonResponse);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON解析失败: {e.Message}\n响应内容：{jsonResponse}");
            return null;
        }
    }

    // 可序列化数据结构
    [System.Serializable]
    private class ChatRequest
    {
        public string model;
        public List<Message> messages;
        public float temperature;
        public int max_tokens;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class DeepSeekResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }
}