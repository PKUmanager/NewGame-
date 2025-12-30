using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem {
    /// <summary>
    /// Handles how and where the data is saved and loaded from
    /// </summary>
    public static class SaveSystem {
        private static readonly string SaveFolder = Application.dataPath;
        private const string SaveFileExtension = ".json";
        private static string FileLocation { get; set; }


        /// <summary>
        /// creates the save directory if not already exists and builds the proper fileName and filePath
        /// </summary>
        public static void Initialize(string filePath, string fileName) {
            if (Directory.Exists(SaveFolder)) {
                Directory.CreateDirectory(SaveFolder);
            }

            FileLocation = SaveFolder + "/" + filePath + "/" + fileName + SaveFileExtension;
        }


        /// <summary>
        /// saves the given SaveData in the specified directory
        /// </summary>
        public static void Save(SaveData saveObject) {
            if (!Directory.Exists(SaveFolder)) {
                Directory.CreateDirectory(SaveFolder);
            }

            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var saveString = JsonConvert.SerializeObject(saveObject, Formatting.None, settings);
            File.WriteAllText(FileLocation, saveString);
        }

        /// <summary>
        /// Loads the saveData from the specified directory
        /// </summary>
        public static SaveData Load() {
            if (File.Exists(FileLocation)) {
                var saveString = File.ReadAllText(FileLocation);
                var loaded = JsonConvert.DeserializeObject<SaveData>(saveString);
                return loaded ?? new SaveData();
            }

            return new SaveData();
        }
    }
}