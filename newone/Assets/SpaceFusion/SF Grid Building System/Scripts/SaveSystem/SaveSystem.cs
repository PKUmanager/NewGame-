using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem
{
    public static class SaveSystem
    {
        // [修改 1] 使用 persistentDataPath，这是全平台通用的可读写目录
        private static readonly string SaveFolder = Application.persistentDataPath;
        private const string SaveFileExtension = ".json";

        // 我们定死文件名为 "SaveFile"，方便 Resources 读取
        // 注意：这里需要你后续在 Unity 编辑器里做配合操作（见第二步）
        private const string DefaultSaveFileName = "SaveFile";

        private static string FileLocation
        {
            get { return Path.Combine(SaveFolder, DefaultSaveFileName + SaveFileExtension); }
        }

        public static void Initialize(string filePath, string fileName)
        {
            // 在 persistentDataPath 下创建文件夹
            if (!Directory.Exists(SaveFolder))
            {
                Directory.CreateDirectory(SaveFolder);
            }
            // FileLocation 属性会自动处理路径，这里不需要额外赋值了，或者你可以保留逻辑做子文件夹支持
        }

        public static void Save(SaveData saveObject)
        {
            if (!Directory.Exists(SaveFolder))
            {
                Directory.CreateDirectory(SaveFolder);
            }

            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var saveString = JsonConvert.SerializeObject(saveObject, Formatting.None, settings);

            // 写入到手机的可写目录
            File.WriteAllText(FileLocation, saveString);
            Debug.Log($"[SaveSystem] 游戏已保存至: {FileLocation}");
        }

        public static SaveData Load()
        {
            // 1. 优先尝试加载用户手机里的存档
            if (File.Exists(FileLocation))
            {
                try
                {
                    var saveString = File.ReadAllText(FileLocation);
                    var loaded = JsonConvert.DeserializeObject<SaveData>(saveString);
                    return loaded ?? new SaveData();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"读取存档失败，将使用新存档: {e.Message}");
                }
            }

            // 2. [核心修复] 如果没有用户存档（第一次安装），则加载“默认初始场景”
            // 我们通过 Resources.Load 读取打包在游戏里的 SaveFile.json
            Debug.Log("[SaveSystem] 未找到用户存档，正在加载默认初始配置...");

            TextAsset defaultSaveData = Resources.Load<TextAsset>(DefaultSaveFileName);

            if (defaultSaveData != null)
            {
                // 将默认数据转为对象返回
                var loaded = JsonConvert.DeserializeObject<SaveData>(defaultSaveData.text);

                // [可选] 立即在手机目录生成一份存档，方便下次直接读取
                // Save(loaded); 

                return loaded ?? new SaveData();
            }
            else
            {
                Debug.LogWarning($"[SaveSystem] Resources 文件夹下没找到名为 '{DefaultSaveFileName}' 的文件！将加载空场景。");
            }

            return new SaveData();
        }
    }
}