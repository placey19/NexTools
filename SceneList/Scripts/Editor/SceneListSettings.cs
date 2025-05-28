using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

using static Nexcide.EditorUtil;

namespace Nexcide.SceneList {

    class SettingsContent {

        public static GUIContent UseBuildSettings = new("Use build settings", "Populate list from build settings instead of folders");
        public static GUIContent ShowFolderLabels = new("Show folder labels", "Whether or not labels are shown in list");
        public static GUIContent Folders = new("Folders", "List of folders used to populate list");
        public static GUIContent LogLevel = new("Level of logging", "Useful for debugging");
    }

    class SceneListSettings : ScriptableObject {

        public const string AssetsRoot = "Assets/";

        private const string SettingsAssetPath = "ProjectSettings/NexcideSceneList.asset";

        // setting defaults
        private const bool DefaultUseBuildSettings = false;
        private static readonly ReadOnlyCollection<string> DefaultFoldersList = new List<string>(new string[] {
            AssetsRoot
        }).AsReadOnly();
        private const bool DefaultShowFolderLabels = true;

        [SerializeField] private bool _useBuildSettings;
        [SerializeField] private bool _showFolderLabels;
        [SerializeField] private List<string> _foldersList = new();
        [SerializeField] private LogLevel _logLevel;

        private static SceneListSettings _settings;

        public static void Initialize() {
            GetSettings();
        }

        public static bool UseBuildSettings() {
            return GetSettings()._useBuildSettings;
        }

        public static bool ShowFolderLabels() {
            return GetSettings()._showFolderLabels;
        }

        public static List<string> GetFolders() {
            SceneListSettings settings = GetSettings();

            if (settings._foldersList.Count > 0) {
                return settings._foldersList;
            } else {
                return DefaultFoldersList.ToList();
            }
        }

        public static LogLevel GetLogLevel() {
            return GetSettings()._logLevel;
        }

        private static SceneListSettings GetSettings() {
            if (_settings == null) {
                _settings = LoadSettings<SceneListSettings>(SettingsAssetPath);

                if (_settings != null) {
                    _settings.name = nameof(SceneListSettings);

                    Log.d(_settings._logLevel, _settings, "Loaded");
                } else {
                    _settings = CreateInstance<SceneListSettings>();
                    _settings.name = nameof(SceneListSettings);
                    SerializedObject obj = new(_settings);
                    ResetToDefaults(obj);
                    obj.ApplyModifiedProperties();
                    SaveSettings(_settings, SettingsAssetPath);

                    Log.i(_settings._logLevel, _settings, $"Created: {SettingsAssetPath}");
                }
            }

            return _settings;
        }

        private static void ResetToDefaults(SerializedObject obj) {
            obj.FindProperty(nameof(_useBuildSettings)).boolValue = DefaultUseBuildSettings;
            obj.FindProperty(nameof(_logLevel)).enumValueIndex = (int)LogLevel.Default;

            SerializedProperty foldersProperty = obj.FindProperty(nameof(_foldersList));
            foldersProperty.ClearArray();
            for (int i = 0; i < DefaultFoldersList.Count; ++i) {
                foldersProperty.InsertArrayElementAtIndex(i);
                foldersProperty.GetArrayElementAtIndex(i).stringValue = DefaultFoldersList[i];
            }

            obj.FindProperty(nameof(_showFolderLabels)).boolValue = DefaultShowFolderLabels;
        }

        public static void OnGUI() {
            SceneListSettings settings = GetSettings();
            SerializedObject obj = new(settings);

            EditorGUILayout.Separator();

            float cachedLabelWidth = EditorGUIUtility.labelWidth;
            float cachedFieldWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.labelWidth = SceneListSettingsStyles.SettingLabelWidth;
            EditorGUIUtility.fieldWidth = SceneListSettingsStyles.SettingFieldWidth;

            PropertyField(obj, nameof(_useBuildSettings), SettingsContent.UseBuildSettings);
            EditorGUILayout.Separator();

            using (new EditorGUI.DisabledGroupScope(settings._useBuildSettings)) {
                EditorGUIUtility.fieldWidth = SceneListSettingsStyles.FolderFieldWidth;
                PropertyField(obj, nameof(_foldersList), SettingsContent.Folders);
                EditorGUILayout.Separator();

                PropertyField(obj, nameof(_showFolderLabels), SettingsContent.ShowFolderLabels);
                EditorGUILayout.Separator();
            }

            EditorGUIUtility.fieldWidth = SceneListSettingsStyles.LogLevelFieldWidth;
            PropertyField(obj, nameof(_logLevel), SettingsContent.LogLevel);
            EditorGUILayout.Separator();

            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Reset to defaults")) {
                    ResetToDefaults(obj);
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUIUtility.labelWidth = cachedLabelWidth;
            EditorGUIUtility.fieldWidth = cachedFieldWidth;

            if (obj.ApplyModifiedProperties()) {
                SaveSettings(settings, SettingsAssetPath);
                SceneListWindow.RefreshIfActive();
            }
        }

        public static void UndoRedoPerformed() {
            SaveSettings(GetSettings(), SettingsAssetPath);
            SceneListWindow.RefreshIfActive();
        }

        private static void PropertyField(SerializedObject obj, string propertyName, GUIContent label) {
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.PropertyField(obj.FindProperty(propertyName), label);
                GUILayout.FlexibleSpace();
            }
        }
    }
}
