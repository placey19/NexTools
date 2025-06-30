using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nexcide.SceneList {

    public class SceneListWindow : EditorWindow, IHasCustomMenu {

        [MenuItem("Nexcide/Scene List", false, 3)]
        private static void Initialize() {
            EditorWindow editorWindow = GetWindow<SceneListWindow>(typeof(SceneListWindow));
            editorWindow.titleContent = new GUIContent("Scene List");
            editorWindow.Show();
        }

        private struct SceneData {

            public string SearchFolder;
            public string AssetPath;
            public string DisplayName;

            public SceneData(string searchFolder, string assetPath, string displayName) {
                SearchFolder = searchFolder;
                AssetPath = assetPath;
                DisplayName = displayName;
            }
        }

        private static SceneListWindow _instance;

        private readonly List<SceneData> _sceneDataList = new();
        private Vector3 _scrollPosition;
        private GUIStyle _buttonStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _vScrollbarStyle;
        private GUIStyle _hScrollbarStyle;
        private bool _refresh;

        private LogLevel LogLevel => SceneListSettings.GetLogLevel();

        private string AssetsRoot => SceneListSettings.AssetsRoot;

        private void OnEnable() {
            name = nameof(SceneListWindow);
            Log.v(LogLevel, this, "OnEnable()");

            SceneListSettings.Initialize();
            _instance = this;
            EditorSceneManager.activeSceneChanged += OnActiveSceneChanged;
            _refresh = true;
        }

        private void OnDisable() {
            Log.v(LogLevel, this, "OnDisable()");

            EditorSceneManager.activeSceneChanged -= OnActiveSceneChanged;
            _instance = null;
        }

        public static void RefreshIfActive() {
            if (_instance != null) {
                _instance._refresh = true;
                _instance.Repaint();
            }
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Refresh"), false, () => _refresh = true);
        }

        private void OnActiveSceneChanged(Scene previous, Scene current) {
            _refresh = true;
            Repaint();
        }

        private void OnGUI() {
            if (_refresh) {
                RefreshScenes();
                _refresh = false;
            }

            float padding = -2.0f;
            float x = padding;
            float y = padding;
            float width = (position.width - padding * 2.0f);
            float height = (position.height - padding * 2.0f);
            Rect area = new(x, y, width, height);

            using (new GUILayout.AreaScope(area)) {
                using (new GUILayout.VerticalScope()) {
                    using GUILayout.ScrollViewScope scrollViewScope = new(_scrollPosition, _vScrollbarStyle, _hScrollbarStyle);
                    _scrollPosition = scrollViewScope.scrollPosition;

                    string searchFolder = AssetsRoot;
                    foreach (SceneData sceneData in _sceneDataList) {
                        if (SceneListSettings.ShowFolderLabels()) {
                            if (sceneData.SearchFolder != searchFolder) {
                                searchFolder = sceneData.SearchFolder;
                                AddLabel(sceneData);
                            }
                        }

                        AddButton(in sceneData);
                    }
                }
            }
        }

        private void AddLabel(in SceneData sceneData) {
            GUILayout.Space(8.0f);

            // get folder without it starting with "Assets/"
            string folder = sceneData.SearchFolder;
            string label = (folder.StartsWith(AssetsRoot) ? folder[AssetsRoot.Length..] : folder);

            // label is actually a button, when clicked, highlight the path in the Project window
            if (GUILayout.Button(label, _labelStyle)) {
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(sceneData.AssetPath);
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }

        private void AddButton(in SceneData sceneData) {
            if (GUILayout.Button(sceneData.DisplayName, _buttonStyle)) {
                Event e = Event.current;

                if (e.button == 0) {
                    // on left click, load the scene
                    if (Application.isPlaying) {
                        SceneManager.LoadScene(sceneData.AssetPath);
                    } else {
                        bool openScene = true;
                        if (SceneListSettings.ShowConfirmation()) {
                            openScene = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        }

                        if (openScene) {
                            Scene openedScene = EditorSceneManager.OpenScene(sceneData.AssetPath, OpenSceneMode.Single);
                            if (!openedScene.IsValid()) {
                                Log.e(this, $"Failed to open scene with path: {sceneData.AssetPath}");
                            }
                        }
                    }
                } else if (e.button == 1) {
                    // on right click, highlight on scene asset in Project window
                    SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneData.AssetPath);
                    Selection.activeObject = sceneAsset;
                    EditorGUIUtility.PingObject(sceneAsset);
                }
            }
        }

        private void RefreshScenes() {
            RefreshStyles();

            if (!SceneListSettings.UseBuildSettings()) {
                Populate();
            } else {
                PopulateFromBuildSettings();
            }

            Log.d(LogLevel, this, $"_sceneDataList.Count: {_sceneDataList.Count}");
        }

        private void RefreshStyles() {
            _vScrollbarStyle = GUI.skin.verticalScrollbar;
            _vScrollbarStyle = GUIStyle.none;
            _hScrollbarStyle = GUI.skin.horizontalScrollbar;
            _hScrollbarStyle = GUIStyle.none;

            _buttonStyle = GUI.skin.button;
            _buttonStyle.alignment = TextAnchor.MiddleLeft;
            _buttonStyle.fontSize = 11;

            _labelStyle = EditorStyles.boldLabel;
            _labelStyle.padding.left = _buttonStyle.padding.left;
        }

        private void Populate() {
            _sceneDataList.Clear();

            foreach (string searchFolder in SceneListSettings.GetFolders()) {
                if (Directory.Exists(searchFolder)) {
                    string[] sceneGuids = AssetDatabase.FindAssets($"t:Scene", new string[] { searchFolder });

                    foreach (string guid in sceneGuids) {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        string displayName = GetSceneName(path, out _);
                        _sceneDataList.Add(new SceneData(searchFolder, path, displayName));
                    }
                } else {
                    Log.w(LogLevel, this, $"Folder doesn't exist: {searchFolder}");
                }
            }
        }

        private void PopulateFromBuildSettings() {
            _sceneDataList.Clear();

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            for (int i = 0; i < scenes.Length; ++i) {
                EditorBuildSettingsScene scene = scenes[i];
                string displayName = GetSceneName(scene.path, out _);
                _sceneDataList.Add(new SceneData(AssetsRoot, scene.path, displayName));
            }
        }

        private static string GetSceneName(string scenePath, out string topFolder) {
            string sceneName;

            string[] items = scenePath.Split('/');

            if (items.Length > 0) {
                sceneName = items[^1];
                sceneName = sceneName[..sceneName.LastIndexOf('.')];
                topFolder = (items.Length > 1 ? items[^2] : "");
            } else {
                sceneName = scenePath;
                topFolder = "";
            }

            return sceneName;
        }
    }
}
