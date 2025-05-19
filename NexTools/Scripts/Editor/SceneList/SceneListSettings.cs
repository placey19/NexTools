using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Nexcide.NexTools {

    class SettingsContent {

        public static GUIContent UseBuildSettings = new("Use build settings", "Populate list from build settings instead of folders");
        public static GUIContent ShowFolderLabels = new("Show folder labels", "Whether or not labels are shown in list");
        public static GUIContent Folders = new("Folders", "List of folders used to populate list");
        public static GUIContent LogLevel = new("Level of logging", "Useful for debugging");
    }

    class SceneListSettings : ScriptableObject {

        private const string KeyPrefix = "SceneList_";

        public const string AssetsRoot = "Assets/";

        // settings keys
        private const string KeyUseBuildSettings = KeyPrefix + "UseBuildSettings";
        private const string KeyFoldersList = KeyPrefix + "Folders";
        private const string KeyShowFolderLabels = KeyPrefix + "ShowFolderLabels";
        private const string KeyLogLevel = KeyPrefix + "LogLevel";

        // setting defaults
        private const bool DefaultUseBuildSettings = false;
        private static readonly ReadOnlyCollection<string> DefaultFoldersList = new List<string>(new string[] {
            AssetsRoot
        }).AsReadOnly();
        private const bool DefaultShowFolderLabels = true;

        [SerializeField] private bool _useBuildSettings;
        [SerializeField] private bool _showFolderLabels;
        [SerializeField] private List<string> _foldersList;
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
                _settings = ScriptableObject.CreateInstance<SceneListSettings>();
                _settings.Load();
            }

            return _settings;
        }

        private void Load() {
            name = nameof(SceneListSettings);

            _useBuildSettings = EditorPrefs.GetBool(KeyUseBuildSettings, DefaultUseBuildSettings);
            _logLevel = (LogLevel)EditorPrefs.GetInt(KeyLogLevel, (int)LogLevel.Default);

            string joinedFoldersList = EditorPrefs.GetString(KeyFoldersList, string.Join(Path.PathSeparator, DefaultFoldersList));
            Log.v(_logLevel, this, $"Load(), joinedFoldersList: {joinedFoldersList}");
            _foldersList = new List<string>(joinedFoldersList.Split(Path.PathSeparator));

            _showFolderLabels = EditorPrefs.GetBool(KeyShowFolderLabels, DefaultShowFolderLabels);

            Log.d(_logLevel, this, "Loaded");
        }

        private void Save() {
            EditorPrefs.SetBool(KeyUseBuildSettings, _useBuildSettings);
            EditorPrefs.SetInt(KeyLogLevel, (int)_logLevel);

            string joinedFoldersList = string.Join(Path.PathSeparator, _foldersList);
            Log.v(_logLevel, this, $"Save(), joinedFoldersList: {joinedFoldersList}");
            EditorPrefs.SetString(KeyFoldersList, joinedFoldersList);

            EditorPrefs.SetBool(KeyShowFolderLabels, _showFolderLabels);
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
                settings.Save();
                SceneList.RefreshIfActive();
            }
        }

        public static void UndoRedoPerformed() {
            GetSettings().Save();
            SceneList.RefreshIfActive();
        }

        private static void PropertyField(SerializedObject obj, string propertyName, GUIContent label) {
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.PropertyField(obj.FindProperty(propertyName), label);
                GUILayout.FlexibleSpace();
            }
        }
    }
}
