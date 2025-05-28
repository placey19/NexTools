using UnityEngine;

namespace Nexcide.EasyMaterials {

    static class EasyMaterialSettingsStyles {

        public const float SettingLabelWidth = 260.0f;
        public const float SettingFieldWidth = 160.0f;
        public const float LogLevelFieldWidth = 100.0f;

        public static readonly GUIStyle ScrollButton = new() {
            margin = new RectOffset(0, 0, 0, 0),
            alignment = TextAnchor.MiddleCenter,
            stretchHeight = true,
            padding = new RectOffset(2, 2, 2, 2)
        };

        public static readonly GUIStyle MaterialButton = new(GUI.skin.button) {
            padding = new RectOffset(4, 4, 4, 4),
            margin = new RectOffset(0, 0, 0, 0)
        };

        public static readonly GUIStyle MaterialButtonSmall = new(MaterialButton) {
            padding = new RectOffset(3, 3, 3, 3)
        };

        public static readonly GUIStyle MaterialButtonList = new(GUI.skin.button) {
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(0, 0, 0, 0)
        };

        public static readonly GUIStyle MaterialButtonWithText = new(GUI.skin.button) {
            imagePosition = ImagePosition.ImageAbove,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(4, 4, 2, 2),
            wordWrap = false,
            fontSize = 10
        };

        public static readonly GUIStyle MaterialButtonWithTextMedium = new(MaterialButtonWithText) {
            fontSize = 9,
            padding = new RectOffset(3, 3, 2, 2),
        };

        public static readonly GUIStyle MaterialButtonWithTextSmall = new(MaterialButtonWithText) {
            fontSize = 8,
            padding = new RectOffset(2, 2, 2, 2),
        };
    }
}
