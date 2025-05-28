using UnityEditor;
using UnityEngine;

namespace Nexcide.EasyMaterials {

    /// <summary>
    /// Utility methods for the Easy Materials edtior tool.
    /// </summary>
    public static class EasyMaterialUtil {

        private const string Ellipsis = "...";

        // reused by GetTextWidth() so it doesn't need to create a new one each time
        private static readonly GUIContent _textContentCache = new GUIContent();

        /// <summary>
        /// Convert one or more items as parameters into an array.
        /// </summary>
        public static T[] ToArray<T>(params T[] items) {
            return items;
        }

        /// <summary>
        /// Convert integer as hex to color, e.g: 0xffff00 for yellow. Default alpha is 0xff.
        /// </summary>
        public static Color HexColorRGB(int hex, byte a = 0xff) {
            byte r = (byte)(hex >> 16 & 0xff);
            byte g = (byte)(hex >> 8 & 0xff);
            byte b = (byte)(hex & 0xff);
            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// Convert integer as hex to color, e.g: 0xffff00ff for yellow with full alpha.
        /// </summary>
        public static Color HexColorRGBA(int hex) {
            byte r = (byte)(hex >> 24 & 0xff);
            byte g = (byte)(hex >> 16 & 0xff);
            byte b = (byte)(hex >> 8 & 0xff);
            byte a = (byte)(hex & 0xff);
            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// Store a color as a string in EditorPrefs.
        /// </summary>
        public static void EditorPrefsSetColor(string key, Color color) {
            string htmlString = ColorUtility.ToHtmlStringRGBA(color);
            EditorPrefs.SetString(key, htmlString);
        }

        /// <summary>
        /// Get color from EditorPrefs that has been stored as a string.
        /// </summary>
        public static Color EditorPrefsGetColor(string key, Color defaultColor) {
            Color color = defaultColor;

            if (EditorPrefs.HasKey(key)) {
                string htmlColor = "#" + EditorPrefs.GetString(key);

                if (!ColorUtility.TryParseHtmlString(htmlColor, out color)) {
                    Log.e("Failed to parse color from EditorPrefs");
                }
            }

            return color;
        }

        /// <summary>
        /// Get the width of some text using the given style.
        /// </summary>
        public static float GetTextWidth(string text, GUIStyle style) {
            _textContentCache.text = text;
            return style.CalcSize(_textContentCache).x;
        }

        /// <summary>
        /// Clip the text and append ellipsis if its width is greater than the given available width when using the given style.
        /// </summary>
        public static string ClipText(string text, GUIStyle style, float availableWidth) {
            // prevent silliness
            availableWidth = Mathf.Max(availableWidth, 0.0f);

            // clip the text and add ellipsis if it's width is greater than available width
            if (GetTextWidth(text, style) > availableWidth) {
                // first clip 1 more char than the length of the ellipsis text
                int length = Mathf.Max(text.Length - (Ellipsis.Length + 1), 0);
                string clippedText = text.Substring(0, length);

                // remove 1 char at a time until the width of the text fits in the available space
                // this could be done more efficiently by doing a binary chop through the text widths but... meh
                while (GetTextWidth(clippedText + Ellipsis, style) > availableWidth) {
                    clippedText = clippedText.Substring(0, clippedText.Length - 1);

                    // just in case
                    if (clippedText.Length <= 0) {
                        break;
                    }
                }

                return (clippedText + Ellipsis);
            } else {
                return text;
            }
        }
    }
}
