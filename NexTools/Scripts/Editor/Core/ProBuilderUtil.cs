using System;
using System.Reflection;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;

using static Nexcide.NexTools.ReflectionUtil;

namespace Nexcide.NexTools {

    public static class ProBuilderUtil {

        public static class _VertexColorPalette {

            private static Action<Color> _setFaceColors;

            public static void SetFaceColors(Color color) {
                if (_setFaceColors == null) {
                    Type vertexColorPalette = GetTypeFromAssembly("UnityEditor.ProBuilder.VertexColorPalette");
                    Type[] types = new Type[] { typeof(Color) };
                    MethodInfo setFaceColors = vertexColorPalette.GetMethod("SetFaceColors", BindingFlags.Public | BindingFlags.Static, binder: null, types, modifiers: null);
                    _setFaceColors = setFaceColors.CreateDelegate<Action<Color>>();
                }

                if (_setFaceColors != null) {
                    _setFaceColors(color);
                } else {
                    Log.e("Error referencing ProBuilder");
                }
            }
        }

        public static class _ProBuilderToolManager {

            private static Func<SelectMode> _getSelectMode;

            public static SelectMode _GetSelectMode() {
                if (_getSelectMode == null) {
                    Type proBuilderToolManager = GetTypeFromAssembly("UnityEditor.ProBuilder.ProBuilderToolManager");
                    PropertyInfo selectModeProperty = proBuilderToolManager.GetProperty("selectMode", BindingFlags.Public | BindingFlags.Static);
                    MethodInfo getMethod = selectModeProperty.GetGetMethod();
                    _getSelectMode = getMethod.CreateDelegate<Func<SelectMode>>();
                }

                SelectMode mode = SelectMode.None;
                if (_getSelectMode != null) {
                    mode = _getSelectMode();
                } else {
                    Log.e("Error referencing ProBuilder");
                }

                return mode;
            }
        }

        public static class _ProBuilderEditor {

            private static Func<ProBuilderEditor, SceneSelection> _get_m_Hovering;

            public static SceneSelection m_Hovering {
                get {
                    SceneSelection selection = null;

                    if (ProBuilderEditor.instance != null) {
                        if (_get_m_Hovering == null) {
                            Type proBuilderEditor = typeof(ProBuilderEditor);
                            FieldInfo hoveringField = proBuilderEditor.GetField("m_Hovering", BindingFlags.NonPublic | BindingFlags.Instance);
                            _get_m_Hovering = hoveringField.CreateDelegate<ProBuilderEditor, SceneSelection>();
                        }

                        if (_get_m_Hovering != null) {
                            selection = _get_m_Hovering(ProBuilderEditor.instance);
                        } else {
                            Log.e("Error referencing ProBuilder");
                        }
                    }

                    return selection;
                }
            }
        }

        public static class _ColorUtility {

            private static Func<Color, string> _getColorName;

            public static string GetColorName(Color color) {
                if (_getColorName == null) {
                    Type colorUtility = GetTypeFromAssembly("UnityEngine.ProBuilder.ColorUtility");
                    Type[] parameters = new Type[] { typeof(Color) };
                    MethodInfo getColorName = colorUtility.GetMethod("GetColorName", BindingFlags.Public | BindingFlags.Static, binder: null, parameters, modifiers: null);
                    _getColorName = getColorName.CreateDelegate<Func<Color, string>>();
                }

                string name = null;
                if (_getColorName != null) {
                    name = _getColorName(color);
                } else {
                    Log.e("Error referencing ProBuilder");
                }

                return name;
            }
        }

        public static class _EditorUtility {

            private static Action<string> _showNotification;

            public static void ShowNotification(string message) {
                if (_showNotification == null) {
                    Type editorUtility = typeof(EditorUtility);
                    Type[] parameters = new Type[] { typeof(string) };
                    MethodInfo showNotification = editorUtility.GetMethod("ShowNotification", BindingFlags.NonPublic | BindingFlags.Static, binder: null, parameters, modifiers: null);
                    _showNotification = showNotification.CreateDelegate<Action<string>>();
                }

                if (_showNotification != null) {
                    _showNotification(message);
                } else {
                    Log.e("Error referencing ProBuilder");
                }
            }
        }
    }
}
