using UnityEditor;
using UnityEngine;

using static Nexcide.EasyMaterials.EasyMaterialUtil;

namespace Nexcide.EasyMaterials {

    class SettingsContent {

        private const string _groupTooltip = "Colors that are cycled through when selecting scene objects";

        public static GUIContent IncludeChildObjects = new("Include immediate child objects", "Include materials from first-level child objects of selected GameObject");
        public static GUIContent MaterialNameUnderIcon = new("Show material name under icon", "Include the name of the material on the button below the icon");
        public static GUIContent ShowMaterialCount = new("Show material count in status bar", "Show the number of materials in the list at the bottom left of the status bar");
        public static GUIContent MaximumRecentMaterials = new("Maximum recent materials length", "The total allowed amount of recently selected materials (Ignored when loading a list)");
        public static GUIContent IconSize = new("Icon size", "The size of the material icon button");
        public static GUIContent MaterialGroup1Tint = new("Material group 1 tint", _groupTooltip);
        public static GUIContent MaterialGroup2Tint = new("Material group 2 tint", _groupTooltip);
        public static GUIContent MaterialGroup3Tint = new("Material group 3 tint", _groupTooltip);
        public static GUIContent AssetMaterialTint = new("Asset material tint", "The tint used on materials selected in the project window");
        public static GUIContent SelectedTint = new("Selected material tint", "Color used when material has been used");
        public static GUIContent ErrorTint = new("Material error tint", "Used when a material is deleted");
        public static GUIContent DebugLogging = new("Enable debug logging", "Extra console logging that you don't want to see unless you're debugging the tool");
    }

    class EasyMaterialSettings : ScriptableObject {

        private const string KeyPrefix = "EasyMaterials_";

        // settings keys
        public const string KeyIncludeChildObjects = KeyPrefix + "IncludeChildObjects";
        public const string KeyMaterialNameUnderIcon = KeyPrefix + "MaterialNameUnderIcon";
        public const string KeyMaterialCountStatusBar = KeyPrefix + "MaterialCountInStatusBar";
        public const string KeyMaxRecentMaterials = KeyPrefix + "MaxRecentMaterials";
        public const string KeyIconSize = KeyPrefix + "IconSize";
        public const string KeyMaterialGroup1Tint = KeyPrefix + "MaterialGroup1Tint";
        public const string KeyMaterialGroup2Tint = KeyPrefix + "MaterialGroup2Tint";
        public const string KeyMaterialGroup3Tint = KeyPrefix + "MaterialGroup3Tint";
        public const string KeyAssetMaterialTint = KeyPrefix + "AssetMaterialTint";
        public const string KeySelectedTint = KeyPrefix + "SelectedTint";
        public const string KeyErrorTint = KeyPrefix + "ErrorTint";
        public const string KeyDebugLogging = KeyPrefix + "DebugLogging";

        // setting limits
        public const int IconSizeMin = 24;
        public const int IconSizeMax = 128 + 4 + 4;           // texture size from AssetPreview.GetAssetPreview() + button padding
        private const int RecentMaterialsMin = 1;
        private const int RecentMaterialsMax = 1000;

        // setting defaults
        private const bool DefaultIncludeChildObjects = false;
        private const bool DefaultMaterialNameUnderIcon = false;
        private const bool DefaultMaterialCountStatusBar = true;
        private const int DefaultMaxRecentMaterials = 50;
        private const int DefaultIconSize = 64;
        private const bool DefaultDebugLogging = false;

        private static readonly Color[] DefaultMaterialGroupTints = new Color[] {

            new Color32(255, 255, 255, 255),
            new Color32(192, 192, 192, 255),
            new Color32(128, 128, 128, 255)
        };

        public static readonly Color DefaultAssetMaterialTint = HexColorRGB(0x7fd6fc);
        public static readonly Color DefaultSelectedTint = HexColorRGB(0x444444);
        public static readonly Color DefaultErrorTint = HexColorRGB(0xff0000);

        [SerializeField] private bool _includeChildObjects;
        [SerializeField] private bool _materialNameUnderIcon;
        [SerializeField] private bool _materialCountStatusBar;
        [SerializeField] private int _maxRecentMaterials;
        [SerializeField] private int _iconSize;
        [SerializeField] private Color _materialGroup1Tint;
        [SerializeField] private Color _materialGroup2Tint;
        [SerializeField] private Color _materialGroup3Tint;
        [SerializeField] private Color _assetMaterialTint;
        [SerializeField] private Color _selectedTint;
        [SerializeField] private Color _errorTint;
        [SerializeField] private bool _debugLogging;

        private Color[] _materialGroupTints;

        private static EasyMaterialSettings _settings;

        private void OnValidate() {
            // ensure settings don't exceed the predefined limits
            _maxRecentMaterials = Mathf.Clamp(_maxRecentMaterials, RecentMaterialsMin, RecentMaterialsMax);
            _iconSize = Mathf.Clamp(_iconSize, IconSizeMin, IconSizeMax);
        }

        public static void Initialize() {
            GetSettings();
        }

        public static bool IncludeChildObjects() {
            return GetSettings()._includeChildObjects;
        }

        public static bool MaterialNameUnderIcon() {
            return GetSettings()._materialNameUnderIcon;
        }

        public static bool MaterialCountStatusBar() {
            return GetSettings()._materialCountStatusBar;
        }

        public static int MaxRecentMaterials() {
            return GetSettings()._maxRecentMaterials;
        }

        public static int IconSize() {
            return GetSettings()._iconSize;
        }

        public static void IconSize(int iconSize) {
            GetSettings()._iconSize = iconSize;
            EditorPrefs.SetInt(KeyIconSize, iconSize);
            EasyMaterialSettingsProvider.RepaintIfActive();
        }

        public static Color[] MaterialGroupTints() {
            return GetSettings()._materialGroupTints;
        }

        public static Color AssetMaterialTint() {
            return GetSettings()._assetMaterialTint;
        }

        public static Color SelectedTint() {
            return GetSettings()._selectedTint;
        }

        public static Color ErrorTint() {
            return GetSettings()._errorTint;
        }

        private static EasyMaterialSettings GetSettings() {
            if (_settings == null) {
                _settings = ScriptableObject.CreateInstance<EasyMaterialSettings>();
                _settings.Load();

                Log.d("Created and loaded EasyMaterialSettings");
            }

            return _settings;
        }

        private void Load() {
            _includeChildObjects = EditorPrefs.GetBool(KeyIncludeChildObjects, DefaultIncludeChildObjects);
            _materialNameUnderIcon = EditorPrefs.GetBool(KeyMaterialNameUnderIcon, DefaultMaterialNameUnderIcon);
            _materialCountStatusBar = EditorPrefs.GetBool(KeyMaterialCountStatusBar, DefaultMaterialCountStatusBar);
            _maxRecentMaterials = EditorPrefs.GetInt(KeyMaxRecentMaterials, DefaultMaxRecentMaterials);
            _iconSize = EditorPrefs.GetInt(KeyIconSize, DefaultIconSize);
            _materialGroup1Tint = EditorPrefsGetColor(KeyMaterialGroup1Tint, DefaultMaterialGroupTints[0]);
            _materialGroup2Tint = EditorPrefsGetColor(KeyMaterialGroup2Tint, DefaultMaterialGroupTints[1]);
            _materialGroup3Tint = EditorPrefsGetColor(KeyMaterialGroup3Tint, DefaultMaterialGroupTints[2]);
            _assetMaterialTint = EditorPrefsGetColor(KeyAssetMaterialTint, DefaultAssetMaterialTint);
            _selectedTint = EditorPrefsGetColor(KeySelectedTint, DefaultSelectedTint);
            _errorTint = EditorPrefsGetColor(KeyErrorTint, DefaultErrorTint);
            _debugLogging = EditorPrefs.GetBool(KeyDebugLogging, DefaultDebugLogging);
            Log.DebugLogging = _debugLogging;

            UpdateMaterialGroupTintsArray();
        }

        private void Save() {
            EditorPrefs.SetBool(KeyIncludeChildObjects, _includeChildObjects);
            EditorPrefs.SetBool(KeyMaterialNameUnderIcon, _materialNameUnderIcon);
            EditorPrefs.SetBool(KeyMaterialCountStatusBar, _materialCountStatusBar);
            EditorPrefs.SetInt(KeyMaxRecentMaterials, _maxRecentMaterials);
            EditorPrefs.SetInt(KeyIconSize, _iconSize);
            EditorPrefsSetColor(KeyMaterialGroup1Tint, _materialGroup1Tint);
            EditorPrefsSetColor(KeyMaterialGroup2Tint, _materialGroup2Tint);
            EditorPrefsSetColor(KeyMaterialGroup3Tint, _materialGroup3Tint);
            EditorPrefsSetColor(KeyAssetMaterialTint, _assetMaterialTint);
            EditorPrefsSetColor(KeySelectedTint, _selectedTint);
            EditorPrefsSetColor(KeyErrorTint, _errorTint);
            EditorPrefs.SetBool(KeyDebugLogging, _debugLogging);
            Log.DebugLogging = _debugLogging;

            UpdateMaterialGroupTintsArray();
        }

        private static void ResetToDefaults(SerializedObject obj) {
            obj.FindProperty(nameof(_includeChildObjects)).boolValue = DefaultIncludeChildObjects;
            obj.FindProperty(nameof(_materialNameUnderIcon)).boolValue = DefaultMaterialNameUnderIcon;
            obj.FindProperty(nameof(_materialCountStatusBar)).boolValue = DefaultMaterialCountStatusBar;
            obj.FindProperty(nameof(_maxRecentMaterials)).intValue = DefaultMaxRecentMaterials;
            obj.FindProperty(nameof(_iconSize)).intValue = DefaultIconSize;
            obj.FindProperty(nameof(_materialGroup1Tint)).colorValue = DefaultMaterialGroupTints[0];
            obj.FindProperty(nameof(_materialGroup2Tint)).colorValue = DefaultMaterialGroupTints[1];
            obj.FindProperty(nameof(_materialGroup3Tint)).colorValue = DefaultMaterialGroupTints[2];
            obj.FindProperty(nameof(_assetMaterialTint)).colorValue = DefaultAssetMaterialTint;
            obj.FindProperty(nameof(_selectedTint)).colorValue = DefaultSelectedTint;
            obj.FindProperty(nameof(_errorTint)).colorValue = DefaultErrorTint;
            obj.FindProperty(nameof(_debugLogging)).boolValue = DefaultDebugLogging;
            Log.DebugLogging = DefaultDebugLogging;

            _settings.UpdateMaterialGroupTintsArray();
        }

        private void UpdateMaterialGroupTintsArray() {
            _materialGroupTints ??= new Color[3];
            _materialGroupTints[0] = _materialGroup1Tint;
            _materialGroupTints[1] = _materialGroup2Tint;
            _materialGroupTints[2] = _materialGroup3Tint;
        }

        // called by SettingsProvider
        public static void OnGUI() {
            EasyMaterialSettings settings = GetSettings();
            SerializedObject obj = new SerializedObject(settings);

            EditorGUILayout.Separator();

            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EasyMaterialStyles.SettingLabelWidth;

            PropertyField(obj, nameof(_includeChildObjects), SettingsContent.IncludeChildObjects);
            PropertyField(obj, nameof(_materialNameUnderIcon), SettingsContent.MaterialNameUnderIcon);
            PropertyField(obj, nameof(_materialCountStatusBar), SettingsContent.ShowMaterialCount);
            PropertyField(obj, nameof(_maxRecentMaterials), SettingsContent.MaximumRecentMaterials);

            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField(SettingsContent.IconSize, GUILayout.Width(EasyMaterialStyles.SettingLabelWidth));

                SerializedProperty iconSizeProp = obj.FindProperty(nameof(_iconSize));
                int oldIconSize = iconSizeProp.intValue;
                float newIconSize = EditorGUILayout.Slider(oldIconSize, IconSizeMin, IconSizeMax, GUILayout.Width(EasyMaterialStyles.SettingFieldWidth));
                iconSizeProp.intValue = Mathf.RoundToInt(newIconSize);
            }

            PropertyField(obj, nameof(_materialGroup1Tint), SettingsContent.MaterialGroup1Tint);
            PropertyField(obj, nameof(_materialGroup2Tint), SettingsContent.MaterialGroup2Tint);
            PropertyField(obj, nameof(_materialGroup3Tint), SettingsContent.MaterialGroup3Tint);
            PropertyField(obj, nameof(_assetMaterialTint), SettingsContent.AssetMaterialTint);
            PropertyField(obj, nameof(_selectedTint), SettingsContent.SelectedTint);
            PropertyField(obj, nameof(_errorTint), SettingsContent.ErrorTint);
            PropertyField(obj, nameof(_debugLogging), SettingsContent.DebugLogging);

            EditorGUILayout.Separator();

            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Reset to defaults")) {
                    ResetToDefaults(obj);
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUIUtility.labelWidth = previousLabelWidth;

            bool modified = obj.hasModifiedProperties;
            obj.ApplyModifiedProperties();

            if (modified) {
                settings.Save();
                EasyMaterialTool.RepaintIfActive();
            }
        }

        public static void UndoRedoPerformed() {
            GetSettings().Save();
            EasyMaterialTool.RepaintIfActive();
        }

        private static void PropertyField(SerializedObject obj, string propertyName, GUIContent label) {
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.PropertyField(obj.FindProperty(propertyName), label);
                GUILayout.FlexibleSpace();
            }
        }
    }
}
