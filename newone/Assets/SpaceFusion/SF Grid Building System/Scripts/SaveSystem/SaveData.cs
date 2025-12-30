using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem {
    /// <summary>
    /// This class holds the information of all placeable objects that are instantiated across the scene
    /// </summary>
    [Serializable]
    public class SaveData {
        public Dictionary<string, PlaceableObjectData> placeableObjectDataCollection = new();


        public void AddData(Data data) {
            if (data is PlaceableObjectData pod) {
                placeableObjectDataCollection[pod.guid] = pod;
            }
        }

        public void RemoveData(Data data) {
            if (data is PlaceableObjectData pod && placeableObjectDataCollection.ContainsKey(pod.guid)) {
                placeableObjectDataCollection.Remove(pod.guid);
            }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context) {
            placeableObjectDataCollection ??= new Dictionary<string, PlaceableObjectData>();
        }
    }
}