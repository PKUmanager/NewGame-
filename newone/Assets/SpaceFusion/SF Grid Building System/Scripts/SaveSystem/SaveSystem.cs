using System.IO;
using Newtonsoft.Json;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // 仅在编辑器下使用，打包不会报错
#endif

namespace SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem
{
    public static class SaveSystem
    {
        // 手机/PC 玩家的本地存档路径 (沙盒目录)
        private static readonly string SaveFolder = Application.persistentDataPath;
        private const string SaveFileExtension = ".json";
        private const string DefaultSaveFileName = "SaveFile";

        // 玩家存档的完整路径
        private static string PlayerSaveLocation
        {
            get { return Path.Combine(SaveFolder, DefaultSaveFileName + SaveFileExtension); }
        }

        public static void Initialize(string filePath, string fileName)
        {
            if (!Directory.Exists(SaveFolder)) Directory.CreateDirectory(SaveFolder);
        }

        // ==========================================
        // 1. 玩家用的保存功能 (存到手机/电脑本地)
        // ==========================================
        public static void Save(SaveData saveObject)
        {
            if (!Directory.Exists(SaveFolder)) Directory.CreateDirectory(SaveFolder);

            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var saveString = JsonConvert.SerializeObject(saveObject, Formatting.None, settings);

            File.WriteAllText(PlayerSaveLocation, saveString);
            Debug.Log($"[SaveSystem] 游戏进度已保存至本地: {PlayerSaveLocation}");
        }

        // ==========================================
        // 2. 统一加载功能 (先找本地，没有则找默认)
        // ==========================================
        public static SaveData Load()
        {
            // A. 优先尝试加载玩家的本地进度
            if (File.Exists(PlayerSaveLocation))
            {
                try
                {
                    var saveString = File.ReadAllText(PlayerSaveLocation);
                    return JsonConvert.DeserializeObject<SaveData>(saveString) ?? new SaveData();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"本地存档损坏，尝试加载默认: {e.Message}");
                }
            }

            // B. 如果本地没有存档（第一次玩，或者刚被重置），加载“出厂设置”
            Debug.Log("[SaveSystem] 加载默认初始配置 (Resources)...");
            TextAsset defaultSaveData = Resources.Load<TextAsset>(DefaultSaveFileName);

            if (defaultSaveData != null)
            {
                return JsonConvert.DeserializeObject<SaveData>(defaultSaveData.text) ?? new SaveData();
            }

            Debug.LogWarning("未找到任何存档或默认配置，加载空场景。");
            return new SaveData();
        }

        // ==========================================
        // 3. [开发专用] 设为默认关卡 (仅编辑器有效)
        // ==========================================
#if UNITY_EDITOR
        public static void SaveAsDefaultLevel(SaveData saveObject)
        {
            // 目标路径：Assets/Resources/SaveFile.json
            string resourcesPath = Application.dataPath + "/Resources/" + DefaultSaveFileName + ".json";

            // 1. 保存到项目里 (Git 能追踪的地方)
            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            // 使用 Indented 格式，生成的 JSON 会有换行和缩进，方便你和队友阅读
            var saveString = JsonConvert.SerializeObject(saveObject, Formatting.Indented, settings);
            File.WriteAllText(resourcesPath, saveString);

            // 2. [关键步骤] 删除本地的旧存档！
            // 这样做的好处：下次你按 Play，游戏发现本地没存档，就会强制读取你刚刚保存的“默认关卡”。
            // 解决了“明明修改了默认关卡，怎么进游戏还是旧样子”的问题。
            if (File.Exists(PlayerSaveLocation))
            {
                File.Delete(PlayerSaveLocation);
                Debug.Log("已清除本地旧存档缓存，确保下次读取最新默认配置。");
            }

            // 3. 刷新编辑器
            AssetDatabase.Refresh();

            Debug.Log($"<color=green><b>[开发工具] 成功！当前场景已设为默认关卡！</b></color>\n文件位置: {resourcesPath}\n\n请记得提交到 GitHub！");
        }
#endif
    }
}