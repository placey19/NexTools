using UnityEngine;

namespace Nexcide.EasyMaterials {

    static class EasyMaterialStyles {

        public static readonly float SettingLabelWidth = 260.0f;
        public static readonly float SettingFieldWidth = 160.0f;

        public static readonly GUIStyle ScrollButton = new GUIStyle() {
            margin = new RectOffset(0, 0, 0, 0),
            alignment = TextAnchor.MiddleCenter,
            stretchHeight = true,
            padding = new RectOffset(2, 2, 2, 2)
        };

        public static readonly GUIStyle MaterialButton = new GUIStyle(GUI.skin.button) {
            padding = new RectOffset(4, 4, 4, 4),
            margin = new RectOffset(0, 0, 0, 0)
        };

        public static readonly GUIStyle MaterialButtonSmall = new GUIStyle(MaterialButton) {
            padding = new RectOffset(3, 3, 3, 3)
        };

        public static readonly GUIStyle MaterialButtonList = new GUIStyle(GUI.skin.button) {
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(0, 0, 0, 0)
        };

        public static readonly GUIStyle MaterialButtonWithText = new GUIStyle(GUI.skin.button) {
            imagePosition = ImagePosition.ImageAbove,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(4, 4, 2, 2),
            wordWrap = false,
            fontSize = 10
        };

        public static readonly GUIStyle MaterialButtonWithTextMedium = new GUIStyle(MaterialButtonWithText) {
            fontSize = 9,
            padding = new RectOffset(3, 3, 2, 2),
        };

        public static readonly GUIStyle MaterialButtonWithTextSmall = new GUIStyle(MaterialButtonWithText) {
            fontSize = 8,
            padding = new RectOffset(2, 2, 2, 2),
        };
    }
}
