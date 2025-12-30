using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables {
    [CreateAssetMenu(fileName = "PlaceableObjectsDatabase", menuName = "SF Studio/Grid System/Placeable Object Database")]
    public class PlaceableObjectDatabase : ScriptableObject {
        public List<Placeable> placeableObjects;

        public Placeable GetPlaceable(string assetIdentifier) {
            return placeableObjects.Find(obj => obj.GetAssetIdentifier() == assetIdentifier);
        }

        public List<Placeable> GetGroupedByObjectGroup(ObjectGroup group) {
            return placeableObjects.FindAll(obj => obj.ObjectGroupInfo == group);
        }
    }
}