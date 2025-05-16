using UnityEditor;
using UnityEngine;

namespace Nexcide {

    /// <summary>
    /// Snapping utility that prioritizes ProGrids over EditorSnapSettings.
    /// </summary>
    public static class SnappingUtil {

        public static bool SnapEnabled() {
            bool snapEnabled = ProGridsUtil.SnapEnabled();

            if (!snapEnabled) {
                snapEnabled = EditorSnapSettings.gridSnapEnabled;
            }

            return snapEnabled;
        }

        public static Vector3 MoveSnap(Vector3 point, SnapAxis axis = SnapAxis.All) {
            return Snapping.Snap(point, GetSnapVector(), axis);
        }

        public static float MoveSnap(float value) {
            return Snapping.Snap(value, GetSnapValue());
        }

        public static Vector3 GetSnapVector() {
            Vector3 snapValue = Vector3.zero;

            if (ProGridsUtil.SnapValue(out float proGridsSnapValue)) {
                snapValue = new Vector3(proGridsSnapValue, proGridsSnapValue, proGridsSnapValue);
            } else {
                snapValue = EditorSnapSettings.move;
            }

            return snapValue;
        }

        public static float GetSnapValue() {
            float snapValue = 0.0f;

            if (ProGridsUtil.SnapValue(out float proGridsSnapValue)) {
                snapValue = proGridsSnapValue;
            } else {
                snapValue = EditorSnapSettings.move.x;
            }

            return snapValue;
        }
    }
}
