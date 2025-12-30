using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Utils {
    public static class SfMathUtils {
        public static Vector2 RoundToNextMultiple(Vector2 size, float multiple) {
            return new Vector2(
                Mathf.Ceil(size.x / multiple) * multiple,
                Mathf.Ceil(size.y / multiple) * multiple
            );
        }

        public static Vector3 RoundVector(Vector3 vector, int decimalPlaces) {
            var multiplier = Mathf.Pow(10, decimalPlaces);
            return new Vector3(
                Mathf.Round(vector.x * multiplier) / multiplier,
                Mathf.Round(vector.y * multiplier) / multiplier,
                Mathf.Round(vector.z * multiplier) / multiplier
            );
        }


        /// <summary>
        /// Since the provided round functions from System.Math or Mathf are pretty useless (.5 values are round to the nearest even integer, instead to the next integer)
        /// we really need to implement our own rounding function....
        /// </summary>
        public static int RoundToInt(float value) {
            return Mathf.Abs(value % 1) == 0.5 ? Mathf.CeilToInt(value) : Mathf.RoundToInt(value);
        }
    }
}