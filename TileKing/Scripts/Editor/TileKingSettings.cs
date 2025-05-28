using UnityEditor;
using UnityEngine;

using static Nexcide.EditorUtil;

namespace Nexcide.TileKing {

    internal class SettingsContent {

        public static GUIContent PrefabAsset = new("Prefab Asset", "Prefab to instantiate from when creating a new mesh");
        public static GUIContent LogLevel = new("Level of logging", "Useful for debugging");
    }

    internal class TileKingSettings : ScriptableObject {

        public const string AssetsRoot = "Assets/";

        private const string SettingsAssetPath = "ProjectSettings/NexcideTileKing.asset";

        // defaults
        private const string DefaultPrefabFileName = "Default Tile King Mesh.prefab";

        [SerializeField] private GameObject _prefabAsset;
        [SerializeField] private LogLevel _logLevel;

        private static TileKingSettings _settings;

        public static GameObject GetPrefabAsset() {
            return GetSettings()._prefabAsset;
        }

        public static LogLevel GetLogLevel() {
            return GetSettings()._logLevel;
        }

        private static TileKingSettings GetSettings() {
            if (_settings == null) {
                _settings = LoadSettings<TileKingSettings>(SettingsAssetPath);

                if (_settings != null) {
                    _settings.name = nameof(TileKingSettings);

                    Log.d(_settings._logLevel, _settings, "Loaded");
                } else {
                    _settings = CreateInstance<TileKingSettings>();
                    _settings.name = nameof(TileKingSettings);
                    SerializedObject obj = new(_settings);
                    _settings.ResetToDefaults(obj);
                    obj.ApplyModifiedProperties();
                    SaveSettings(_settings, SettingsAssetPath);

                    Log.i(_settings._logLevel, _settings, $"Created: {SettingsAssetPath}");
                }
            }

            return _settings;
        }


        private void ResetToDefaults(SerializedObject obj) {
            if (FindDefaultPrefabAsset(out GameObject defaultPrefabAsset)) {
                obj.FindProperty(nameof(_prefabAsset)).objectReferenceValue = defaultPrefabAsset;
            }

            obj.FindProperty(nameof(_logLevel)).enumValueIndex = (int)LogLevel.Default;
        }

        private bool FindDefaultPrefabAsset(out GameObject prefabAsset) {
            const string defaultPrefabAssetPath = ("TileKing/Prefabs/" + DefaultPrefabFileName);

            prefabAsset = null;

            string[] assetGuids = AssetDatabase.FindAssets("t:Prefab");
            if (assetGuids.Length > 0) {
                foreach (string guid in assetGuids) {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    if (assetPath.EndsWith(defaultPrefabAssetPath)) {
                        prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        break;
                    }
                }
            }

            if (prefabAsset == null) {
                Log.e(_logLevel, this, $"Failed to find default prefab asset at path ending with: {defaultPrefabAssetPath}");
            }

            return (prefabAsset != null);
        }

        public static void OnGUI() {
            TileKingSettings settings = GetSettings();
            SerializedObject obj = new(settings);

            GameObject currentPrefabAsset = settings._prefabAsset;
            EditorGUILayout.Separator();

            float cachedLabelWidth = EditorGUIUtility.labelWidth;
            float cachedFieldWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.labelWidth = TileKingSettingsStyles.SettingLabelWidth;
            EditorGUIUtility.fieldWidth = TileKingSettingsStyles.SettingFieldWidth;

            PropertyField(obj, nameof(_prefabAsset), SettingsContent.PrefabAsset);
            EditorGUILayout.Separator();

            EditorGUIUtility.fieldWidth = TileKingSettingsStyles.LogLevelFieldWidth;
            PropertyField(obj, nameof(_logLevel), SettingsContent.LogLevel);
            EditorGUILayout.Separator();

            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Reset to defaults")) {
                    settings.ResetToDefaults(obj);
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUIUtility.labelWidth = cachedLabelWidth;
            EditorGUIUtility.fieldWidth = cachedFieldWidth;

            if (obj.hasModifiedProperties) {
                settings.ValidatePrefabAsset(obj, currentPrefabAsset);

                obj.ApplyModifiedProperties();
                SaveSettings(settings, SettingsAssetPath);
            }
        }

        private void ValidatePrefabAsset(SerializedObject obj, GameObject currentPrefabAsset) {
            SerializedProperty prefabAssetProperty = obj.FindProperty(nameof(_prefabAsset));

            // if prefab asset has changed
            if (prefabAssetProperty.objectReferenceValue != currentPrefabAsset) {
                bool resetPrefabAssetToDefault = false;

                if (prefabAssetProperty.objectReferenceValue is GameObject prefabAsset) {
                    if (AssetDatabase.IsMainAsset(prefabAsset)) {
                        if (!prefabAsset.TryGetComponent(out TileKingMesh _)) {
                            Log.e(_settings._logLevel, _settings, $"Prefab asset must have a {nameof(TileKingMesh)} behaviour");
                            resetPrefabAssetToDefault = true;
                        }
                    } else {
                        Log.e(_settings._logLevel, _settings, "Can't assign non-main prefab");
                        resetPrefabAssetToDefault = true;
                    }
                } else {
                    resetPrefabAssetToDefault = true;
                }

                if (resetPrefabAssetToDefault && _settings.FindDefaultPrefabAsset(out GameObject defaultPrefabAsset)) {
                    prefabAssetProperty.objectReferenceValue = defaultPrefabAsset;
                }
            }
        }

        public static void UndoRedoPerformed() {
            SaveSettings(GetSettings(), SettingsAssetPath);
        }

        private static void PropertyField(SerializedObject obj, string propertyName, GUIContent label) {
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.PropertyField(obj.FindProperty(propertyName), label);
                GUILayout.FlexibleSpace();
            }
        }
    }
}
