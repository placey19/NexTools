using UnityEditor;
using UnityEngine;

using static Nexcide.EasyMaterials.EasyMaterialUtil;
using static Nexcide.EditorUtil;

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

        private const string SettingsAssetPath = "ProjectSettings/NexcideEasyMaterials.asset";

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

        private static EasyMaterialSettings _settings;

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
            EasyMaterialSettings settings = GetSettings();
            settings._iconSize = iconSize;
            SaveSettings(settings, SettingsAssetPath);

            EasyMaterialSettingsProvider.RepaintIfActive();
        }

        public static Color[] MaterialGroupTints() {
            return GetSettings().GetMaterialGroupTintsArray();
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
                _settings = LoadSettings<EasyMaterialSettings>(SettingsAssetPath);

                if (_settings != null) {
                    Log.d($"Loaded {nameof(EasyMaterialSettings)}");
                } else {
                    _settings = CreateInstance<EasyMaterialSettings>();
                    SerializedObject obj = new(_settings);
                    ResetToDefaults(obj);
                    obj.ApplyModifiedProperties();
                    SaveSettings(_settings, SettingsAssetPath);

                    Log.i($"Created: {SettingsAssetPath}");
                }

                Log.DebugLogging = _settings._debugLogging;
            }

            return _settings;
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
        }

        private Color[] GetMaterialGroupTintsArray() {
            _materialGroupTints ??= new Color[3];
            _materialGroupTints[0] = _materialGroup1Tint;
            _materialGroupTints[1] = _materialGroup2Tint;
            _materialGroupTints[2] = _materialGroup3Tint;

            return _materialGroupTints;
        }

        // called by SettingsProvider
        public static void OnGUI() {
            EasyMaterialSettings settings = GetSettings();
            SerializedObject obj = new(settings);

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
                SaveSettings(settings, SettingsAssetPath);
                EasyMaterialWindow.RepaintIfActive();
            }
        }

        public static void UndoRedoPerformed() {
            SaveSettings(GetSettings(), SettingsAssetPath);
            EasyMaterialWindow.RepaintIfActive();
        }

        private static void PropertyField(SerializedObject obj, string propertyName, GUIContent label) {
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.PropertyField(obj.FindProperty(propertyName), label);
                GUILayout.FlexibleSpace();
            }
        }
    }
}
