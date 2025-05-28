using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Nexcide {

    public class EditorUtil {

        public enum PrefabUnpack {

            None,
            OutermostRoot,
            Completely
        }

        /// <summary>
        /// Add a header label in the same style as the [Header("Header")] attribute.
        /// </summary>
        public static void AddHeader(string header) {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
        }

        public static bool TextFieldFloat(float value, out float newValue, params GUILayoutOption[] options) {
            newValue = 0.0f;
            bool changed = false;
            string valueString = EditorGUILayout.TextField(value.ToString(), options);

            try {
                newValue = float.Parse(valueString);
                changed = (newValue != value);
            } catch (FormatException) {
                // ignore
            }

            return changed;
        }

        public static bool TextFieldInt(int value, out int newValue, params GUILayoutOption[] options) {
            newValue = 0;
            bool changed = false;
            string valueString = EditorGUILayout.TextField(value.ToString(), options);

            try {
                newValue = int.Parse(valueString);
                changed = (newValue != value);
            } catch (FormatException) {
                // ignore
            }

            return changed;
        }

        public static T LoadSettings<T>(string path) where T : ScriptableObject {
            UnityEngine.Object[] objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
            return (objs.Length > 0 ? objs[0] : null) as T;
        }

        public static void SaveSettings(ScriptableObject obj, string path) {
            string folderPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }

            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { obj }, path, allowTextSerialization: true);
        }

        /// <summary>
        /// Spawn a prefab asset at the pivot of the last active SceneView, with undo support.
        /// </summary>
        /// <param name="prefabAsset">The asset GameObject to instantiate</param>
        /// <param name="undoName">Name given to the undo option</param>
        /// <param name="positionOffset">Instantiated offset position</param>
        /// <param name="unpack">Whether or not to unpack the prefab completely</param>
        /// <returns>Newly instantiated prefab GameObject instance</returns>
        public static GameObject SpawnPrefab(GameObject prefabAsset, string undoName, Vector3 positionOffset, PrefabUnpack unpack) {
            GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            gameObject.transform.SetAsLastSibling();

            switch (unpack) {
                case PrefabUnpack.OutermostRoot: {
                    PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                    break;
                }

                case PrefabUnpack.Completely: {
                    PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    break;
                }
            }

            // position the new GameObject at the pivot point of the last active SceneView
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null) {
                gameObject.transform.position = SnappingUtil.MoveSnap(sceneView.pivot + positionOffset);
            }

            // register undo and select the new game object
            Undo.RegisterCreatedObjectUndo(gameObject, undoName);
            Selection.activeGameObject = gameObject;

            return gameObject;
        }
    }
}
