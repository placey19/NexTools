using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using static Nexcide.EasyMaterials.EasyMaterialUtil;
using static UnityEditor.EditorGUILayout;

namespace Nexcide.EasyMaterials {

    public class EasyMaterialWindow : EditorWindow, IHasCustomMenu {

        private const string Version = "1.0";
        private const string WindowTitle = "Easy Materials";
        private const string DefaultStatusText = (WindowTitle + " v" + Version);

        private const float GridSpacing = 2.0f;
        private const float ScrollButtonHeight = 20.0f;         // height only when in vertical mode
        private const float ScrollButtonWidth = 20.0f;          // width only when in horizontal mode
        private const float ScrollButtonStep = 3.0f * 20.0f;    // experimentation showed mousewheel delta was 3 and total movement 60, maybe 20 is a config option somewhere?
        private const float StatusBarHeight = 20.0f;

        private static EasyMaterialWindow _instance;

        // scroll button resources
        private Texture _scrollTextureUp;
        private Texture _scrollTextureDown;
        private Texture _scrollTextureLeft;
        private Texture _scrollTextureRight;

        // functional variables
        [SerializeField] private MaterialDataCollection _materials;
        [SerializeField] private bool _locked;
        [SerializeField] private GUIStyle _padlockButtonStyle;
        [SerializeField] private string _statusText = DefaultStatusText;
        [SerializeField] private Vector2 _scrollPosition;
        [SerializeField] private bool _ignoreNextSelectionChange;
        [SerializeField] private bool _showScrollButtons;
        [SerializeField] private int _tintIndex;

        private MaterialData _selected;
        private string _statusTextRight;
        private int _hoveredIndex;
        private bool _dragging;

        public LogLevel LogLevel => EasyMaterialSettings.GetLogLevel();

        [MenuItem("Nexcide/Easy Materials", false, 1)]
        private static void ShowWindow() {
            EasyMaterialWindow window = GetWindow<EasyMaterialWindow>(WindowTitle);
            window.Show();
        }

        public static void RepaintIfActive() {
            if (_instance != null) {
                _instance.Repaint();
            }
        }

        private void OnEnable() {
            name = nameof(EasyMaterialWindow);
            EasyMaterialSettings.Initialize();
            Log.v(LogLevel, this, "OnEnable()");

            _instance = this;

            // might have been serialized
            if (_materials != null) {
                _materials.OnEnable();
            } else {
                _materials = new MaterialDataCollection();
                _materials.OnEnable();
            }

            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.newSceneCreated += OnNewSceneCreated;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            EditorApplication.projectChanged += ProjectChanged;

            _statusTextRight = null;

            _scrollTextureUp = Resources.Load<Texture2D>("EmIconUp");
            _scrollTextureDown = Resources.Load<Texture2D>("EmIconDown");
            _scrollTextureLeft = Resources.Load<Texture2D>("EmIconLeft");
            _scrollTextureRight = Resources.Load<Texture2D>("EmIconRight");
        }

        private void OnDisable() {
            Log.v(LogLevel, this, "OnDisable()");

            _instance = null;
            _materials.OnDisable();

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.newSceneCreated -= OnNewSceneCreated;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.projectChanged -= ProjectChanged;
        }

        private void ProjectChanged() {
            Log.d(LogLevel, this, "ProjectChanged()");

            _materials.Refresh();
            _statusText = "";

            // clear and re-populate the dictionary to be safe
            _materials.RefreshDictionary();
        }

        private void PlayModeStateChanged(PlayModeStateChange state) {
            Log.d(LogLevel, this, $"PlayModeStateChanged(), state: {state}");

            // all material icons need to be refreshed when entering play mode and edit mode (otherwise they end up blank)
            switch (state) {
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.EnteredEditMode: {
                    _materials.Refresh();
                    break;
                }

                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.ExitingPlayMode: {
                    // not used
                    break;
                }
            }
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode) {
            Log.d(LogLevel, this, $"OnSceneOpened(), scene: {scene.name}, mode: {mode}");

            // all materials need to be refreshed when opening a new scene (otherwise they end up blank)
            if (mode == OpenSceneMode.Single) {
                _materials.Refresh();
            }
        }

        private void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode) {
            Log.d(LogLevel, this, $"OnNewSceneCreated(), scene: {scene}, setup: {setup}, mode: {mode}");

            // all materials need to be refreshed when creating a new scene (otherwise they end up blank)
            if (mode == NewSceneMode.Single) {
                _materials.Refresh();
            }
        }

        /// <summary>
        /// Implementation of interface IHasCustomMenu for adding the 'Lock' menu option to partner with the padlock icon.
        /// </summary>
        /// <param name="menu"></param>
        public void AddItemsToMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Lock"), _locked, () => _locked = !_locked);
        }

        /// <summary>
        /// Magic method called by Unity that we can use to display the padlock icon.
        /// Found at: http://leahayes.co.uk/2013/04/30/adding-the-little-padlock-button-to-your-editorwindow.html
        /// </summary>
        private void ShowButton(Rect position) {
            _padlockButtonStyle ??= "IN LockButton";

            _locked = GUI.Toggle(position, _locked, GUIContent.none, _padlockButtonStyle);
        }

        private void OnSelectionChange() {
            if (!_ignoreNextSelectionChange) {
                if (!_locked) {
                    Transform selectedTransform = (Selection.activeGameObject != null ? Selection.activeGameObject.transform : null);

                    if (selectedTransform != null) {
                        Log.d(LogLevel, this, $"OnSelectionChange(), selectedTransform: {selectedTransform}");

                        // if a GameObject has been selected that has a renderer component, add all of its materials to the list
                        int previousTintIndex = _tintIndex;
                        Color color = GetMaterialGroupTint();
                        bool addedMaterials = AddMaterials(selectedTransform, color);

                        // if it has child objects then process them as well, depending on setting
                        if (selectedTransform.childCount > 0 && EasyMaterialSettings.IncludeChildObjects()) {
                            for (int i = 0; i < selectedTransform.childCount; ++i) {
                                // add each material on this child GameObject using the same tint color
                                Transform childTransform = selectedTransform.GetChild(i);
                                addedMaterials |= AddMaterials(childTransform, color);
                            }
                        }

                        // repaint to see updated material immediately, else reset the group tint index since the newly selected color wasn't used
                        if (addedMaterials) {
                            Repaint();
                        } else {
                            _tintIndex = previousTintIndex;
                        }
                    } else if (Selection.activeObject != null) {
                        Log.d(LogLevel, this, $"OnSelectionChange(), Selection.activeObject: {Selection.activeObject}");

                        // if a material has been selected in the project window, add it to the list using color from settings
                        if (Selection.activeObject.GetType() == typeof(Material)) {
                            Material material = (Material)Selection.activeObject;
                            AddMaterial(material, EasyMaterialSettings.AssetMaterialTint());

                            Repaint();
                        }
                    }
                }
            } else {
                _ignoreNextSelectionChange = false;
            }
        }

        private void OnGUI() {
            Event e = Event.current;
            bool showScrollButtons = _showScrollButtons;

            // draw status bar first, otherwise (for some reason) using the slider gets interrupted when the scroll bars appear
            DrawStatusBar(e);

            float width = position.width;
            float height = (position.height - StatusBarHeight);
            Rect areaRect = new(0.0f, 0.0f, width, height);

            // handle objects being dragged and dropped into the area
            HandleDragAndDrop(areaRect);

            using (new GUILayout.AreaScope(areaRect)) {
                bool list = (EasyMaterialSettings.IconSize() == EasyMaterialSettings.IconSizeMin);
                bool vertical = (list || (position.width <= position.height));

                if (vertical) {
                    if (_showScrollButtons) {
                        DrawTopScrollButton();
                    }

                    DrawMaterialsVertical(out float scrollEnd, out showScrollButtons, list);

                    if (_showScrollButtons) {
                        DrawBottomScrollButton(scrollEnd);
                    }
                } else {
                    using (new HorizontalScope()) {
                        if (_showScrollButtons) {
                            DrawLeftScrollButton();
                        }

                        DrawMaterialsHorizontal(out float scrollEnd, out showScrollButtons);

                        if (_showScrollButtons) {
                            DrawRightScrollButton(scrollEnd);
                        }

                        // handle mouse wheel
                        if (e.type == EventType.ScrollWheel && areaRect.Contains(e.mousePosition)) {
                            _scrollPosition.x += (Event.current.delta.y * 20.0f);
                            _scrollPosition.x = Mathf.Clamp(_scrollPosition.x, 0.0f, scrollEnd);
                            Repaint();
                        }
                    }
                }

                if (e.type == EventType.DragExited) {
                    _dragging = false;
                }
            }

            // the methods above determine whether or not to show the scroll buttons but we can't change the value
            // until after everything else has been drawn, or else errors occur due to mismatched control count.
            // also trigger a repaint so the scroll bars are added or removed immediately
            if (showScrollButtons != _showScrollButtons) {
                _showScrollButtons = showScrollButtons;
                Repaint();
            }
        }

        private void HandleDragAndDrop(Rect dropRect) {
            Event e = Event.current;

            if ((e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && dropRect.Contains(e.mousePosition)) {
                UnityEngine.Object[] objects = DragAndDrop.objectReferences;

                if (objects.Length > 0) {
                    bool handled = HandleDragMaterialBundles(e, objects);

                    if (!handled) {
                        HandleDragMaterials(e, objects);
                    }
                }
            }
        }

        private bool HandleDragMaterialBundles(Event e, UnityEngine.Object[] objects) {
            bool handled = false;

            // try and convert array of objects into array of MaterialBundles, if any object is not a MaterialBundle, exception is thrown
            try {
                MaterialBundle[] materialBundles = objects.Cast<MaterialBundle>().ToArray();

                // if the user is holding the CTRL key, append to the list instead of clearing it
                bool append = false;
                if ((e.modifiers & EventModifiers.Control) == EventModifiers.Control) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    append = true;
                } else if (e.modifiers == EventModifiers.None) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                }

                if (e.type == EventType.DragPerform) {
                    DragAndDrop.AcceptDrag();
                    LoadMaterialsFromAssets(materialBundles, append);
                }

                handled = true;
            } catch (InvalidCastException) {
                // ignore invalid array of objects
            }

            return handled;
        }

        private bool HandleDragMaterials(Event e, UnityEngine.Object[] objects) {
            bool handled = false;

            // try and convert array of objects into array of Materials, if any object is not a Material, exception is thrown
            try {
                Material[] materials = objects.Cast<Material>().ToArray();

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (e.type == EventType.DragPerform) {
                    DragAndDrop.AcceptDrag();

                    Log.d(LogLevel, this, $"Inserting {materials.Length} materials at index: {_hoveredIndex}");
                    foreach (Material material in materials) {
                        _materials.InsertAt(_hoveredIndex, material, EasyMaterialSettings.AssetMaterialTint());
                    }
                }

                handled = true;
            } catch (InvalidCastException) {
                // ignore invalid array of objects
            }

            return handled;
        }

        private void DrawLeftScrollButton() {
            // disable the button if we're fully scrolled left
            GUI.enabled = (_scrollPosition.x > 0.0f);

            if (GUILayout.Button(_scrollTextureLeft, EasyMaterialSettingsStyles.ScrollButton, GUILayout.Width(ScrollButtonWidth))) {
                _scrollPosition.x = Mathf.Max(_scrollPosition.x - ScrollButtonStep, 0.0f);
                Event.current.Use();
            }

            GUI.enabled = true;
        }

        private void DrawRightScrollButton(float scrollEnd) {
            // disable the button if we're fully scrolled right
            GUI.enabled = !(scrollEnd == _scrollPosition.x);

            if (GUILayout.Button(_scrollTextureRight, EasyMaterialSettingsStyles.ScrollButton, GUILayout.Width(ScrollButtonWidth))) {
                _scrollPosition.x = Mathf.Min(_scrollPosition.x + ScrollButtonStep, scrollEnd);
                Event.current.Use();
            }

            GUI.enabled = true;
        }

        private void DrawTopScrollButton() {
            // disable the button if we're fully scrolled up
            GUI.enabled = (_scrollPosition.y > 0.0f);

            if (GUILayout.Button(_scrollTextureUp, EasyMaterialSettingsStyles.ScrollButton, GUILayout.Height(ScrollButtonHeight))) {
                _scrollPosition.y = Mathf.Max(_scrollPosition.y - ScrollButtonStep, 0.0f);
                Event.current.Use();
            }

            GUI.enabled = true;
        }

        private void DrawBottomScrollButton(float scrollEnd) {
            // disable the button if we're fully scrolled down
            GUI.enabled = !(scrollEnd == _scrollPosition.y);

            if (GUILayout.Button(_scrollTextureDown, EasyMaterialSettingsStyles.ScrollButton, GUILayout.Height(ScrollButtonHeight))) {
                _scrollPosition.y += Mathf.Min(_scrollPosition.y + ScrollButtonStep, scrollEnd);
                Event.current.Use();
            }

            GUI.enabled = true;
        }

        private void DrawMaterialsVertical(out float scrollEnd, out bool showScrollButtons, bool list) {
            using VerticalScope scope = new(GUILayout.ExpandHeight(true));
            Event e = Event.current;

            float iconSize = EasyMaterialSettings.IconSize();

            Vector2 cellSize;
            if (list) {
                cellSize = new(scope.rect.width, EasyMaterialSettings.IconSizeMin + GridSpacing);
            } else {
                float size = (iconSize + GridSpacing);
                cellSize = new(size, size);
            }

            // calculate columns, rows and rectangle for the grid of materials
            int columns = (int)(scope.rect.width / cellSize.x);
            int rows = (columns > 0 ? Mathf.CeilToInt((float)_materials.Count / columns) : 1);
            float width = (columns * cellSize.x);
            float height = (rows * cellSize.y);
            Rect viewRect = new(0.0f, 0.0f, width, height);

            // set whether or not to show scroll buttons
            if (e.type == EventType.Repaint) {
                float scrollButtonsHeight = (_showScrollButtons ? ScrollButtonHeight * 2.0f : 0.0f);
                float availableHeight = (scope.rect.height + scrollButtonsHeight);
                float requiredHeight = viewRect.height;
                showScrollButtons = (requiredHeight > availableHeight);
            } else {
                showScrollButtons = _showScrollButtons;
            }

            // set the value for the end of scroll position
            scrollEnd = (viewRect.height - scope.rect.height);

            DrawMaterialButtons(e, scope.rect, viewRect, cellSize, width, list);
        }

        private void DrawMaterialsHorizontal(out float scrollEnd, out bool showScrollButtons) {
            using HorizontalScope scope = new(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            Event e = Event.current;

            float iconSize = EasyMaterialSettings.IconSize();
            Vector2 cellSize = new(iconSize + GridSpacing, iconSize + GridSpacing);

            // calculate columns, rows and rectangle for the grid of materials
            int columns = (int)(scope.rect.width / cellSize.x);
            int rows = Mathf.Max((int)(scope.rect.height / cellSize.y), 1);
            int cellCount = (rows * columns);

            // recalculate columns based on available rows if available cell count won't fit all the materials
            if (columns > 0 && cellCount < _materials.Count) {
                columns = Mathf.CeilToInt((float)_materials.Count / rows);
            }

            float width = (columns * cellSize.x);
            float height = (rows * cellSize.y);
            Rect viewRect = new(0.0f, 0.0f, width, height);

            // set whether or not to show scroll buttons
            if (e.type == EventType.Repaint) {
                float scrollButtonsWidth = (_showScrollButtons ? ScrollButtonWidth * 2.0f : 0.0f);
                float availableWidth = (scope.rect.width + scrollButtonsWidth);
                float requiredWidth = viewRect.width;
                showScrollButtons = (requiredWidth > availableWidth);
            } else {
                showScrollButtons = _showScrollButtons;
            }

            // set the value for the end of scroll position
            scrollEnd = (viewRect.width - scope.rect.width);

            DrawMaterialButtons(e, scope.rect, viewRect, cellSize, width, false);
        }

        private void DrawMaterialButtons(Event e, Rect scopeRect, Rect viewRect, Vector2 cellSize, float width, bool list) {
            using GUI.ScrollViewScope scrollViewScope = new(scopeRect, _scrollPosition, viewRect, GUIStyle.none, GUIStyle.none);
            _scrollPosition = scrollViewScope.scrollPosition;

            // calculate visible rect so we only draw buttons that will actually be visible
            Rect visibleRect = viewRect;
            visibleRect.x += _scrollPosition.x;
            visibleRect.y += _scrollPosition.y;

            // iterate in reverse order through the list so last added is processed first
            Vector2 gridPosition = new(GridSpacing, GridSpacing);
            List<MaterialData> materialDataList = _materials.List();

            bool mouseOverAnyMaterialRect = false;
            for (int i = (materialDataList.Count - 1); i >= 0; --i) {
                MaterialData materialData = materialDataList[i];
                Rect materialRect = new(gridPosition.x, gridPosition.y, cellSize.x - GridSpacing, cellSize.y - GridSpacing);

                bool dragTarget = false;
                bool drawButtonEvent = (e.type == EventType.Repaint || e.type == EventType.MouseDown || e.type == EventType.MouseUp);

                if (drawButtonEvent || e.type == EventType.MouseDrag) {
                    if (HandleMouseForMaterial(e, materialData, materialRect)) {
                        _hoveredIndex = i;
                        dragTarget = _dragging;
                        mouseOverAnyMaterialRect = true;
                    }
                }

                // glorious optimization... only draw buttons during specific events and only if they're visible
                if (drawButtonEvent && visibleRect.Overlaps(materialRect)) {
                    GUIStyle style;
                    GUIContent content;

                    // set button style & content depending on configuration
                    float availableWidth = EasyMaterialSettings.IconSize();
                    if (list) {
                        style = EasyMaterialSettingsStyles.MaterialButtonList;
                        content = new GUIContent(materialData.GetName(), materialData.GetAssetPreview());

                        // calculate space for text on button and if there's not enough space, clip it
                        float iconWidth = materialRect.height;
                        float padding = 4.0f;
                        availableWidth = (position.width - iconWidth - padding);
                        content.text = ClipText(content.text, style, availableWidth);
                    } else if (EasyMaterialSettings.MaterialNameUnderIcon()) {
                        if (availableWidth < 50.0f) {
                            style = EasyMaterialSettingsStyles.MaterialButtonWithTextSmall;
                        } else if (availableWidth < 60.0f) {
                            style = EasyMaterialSettingsStyles.MaterialButtonWithTextMedium;
                        } else {
                            style = EasyMaterialSettingsStyles.MaterialButtonWithText;
                        }

                        string materialName = ClipText(materialData.GetName(), style, availableWidth);
                        content = new GUIContent(materialName, materialData.GetAssetPreview(), materialData.GetName());
                    } else {
                        if (availableWidth < 50.0f) {
                            style = EasyMaterialSettingsStyles.MaterialButtonSmall;
                        } else {
                            style = EasyMaterialSettingsStyles.MaterialButton;
                        }

                        content = new GUIContent(materialData.GetAssetPreview(), materialData.GetName());
                    }

                    DrawMaterialButton(materialData, materialRect, content, style, dragTarget);
                }

                // update grid position
                gridPosition.x += cellSize.x;
                if (gridPosition.x > width) {
                    gridPosition.x = GridSpacing;
                    gridPosition.y += cellSize.y;
                }
            }

            if (e.type == EventType.Repaint && !mouseOverAnyMaterialRect) {
                _hoveredIndex = 0;
            }

            // handle showing context menu when the user doesn't click on a material
            if (e.type == EventType.ContextClick) {
                ShowContextMenu();
            }
        }

        private void ShowContextMenu(MaterialData materialData = null) {
            GenericMenu menu = new();

            if (materialData != null) {
                menu.AddItem(new GUIContent("Remove"), false, () => ContextRemove(materialData));
                menu.AddSeparator("");
            }
            
            menu.AddItem(new GUIContent("Refresh All"), false, () => _materials.Refresh(true));
            menu.AddItem(new GUIContent("Load"), false, ContextLoadList);
            menu.AddItem(new GUIContent("Save"), false, ContextSaveList);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Clear List"), false, ContextClearList);

            menu.ShowAsContext();
        }

        private void ContextRemove(MaterialData materialData) {
            _materials.Remove(materialData);

            if (_materials.Count > 0) {
                _statusText = "";
            } else {
                _statusText = DefaultStatusText;
            }
        }

        private void ContextLoadList() {
            string path = EditorUtility.OpenFilePanel("Load Materials from Asset", "", "asset");

            // path is empty if dialog was dismissed
            if (path.Length > 0) {
                path = FileUtil.GetProjectRelativePath(path);
                LoadMaterialsFromAsset(path);
            }
        }

        private void ContextSaveList() {
            if (_materials.Count > 0) {
                string path = EditorUtility.SaveFilePanel("Save Materials to Asset", "", "Easy Materials.asset", "asset");

                // path is empty if dialog was dismissed
                if (path.Length > 0) {
                    path = FileUtil.GetProjectRelativePath(path);

                    SaveMaterialsToAsset(path);
                }
            } else {
                Log.w(LogLevel, this, "Nothing to save");
            }
        }

        private void ContextClearList() {
            _materials.Clear();
            _statusText = DefaultStatusText;
        }

        private bool HandleMouseForMaterial(Event e, MaterialData materialData, Rect rect) {
            bool contains = rect.Contains(e.mousePosition);

            // if the mouse position is within the rectangle calculated for the given material
            if (contains) {
                // set the status text and request a repaint so status bar is updated immediately
                _statusText = materialData.GetName();
                Repaint();

                // handle dragging of the material object
                if (e.type == EventType.MouseDrag) {
                    materialData.StartDrag();
                    _selected = materialData;
                    _dragging = true;

                    // remove this material as the hot control to allow the scene view to handle the drop operation properly
                    GUIUtility.hotControl = 0;

                    // consume current event
                    e.Use();
                } else if (e.type == EventType.MouseUp && e.button == 1) {
                    ShowContextMenu(materialData);
                }
            }

            return contains;
        }

        private void DrawMaterialButton(MaterialData materialData, Rect rect, GUIContent content, GUIStyle style, bool dragTarget) {
            Color previousBackgroundColor = GUI.backgroundColor;

            if (materialData.IsMissing()) {
                GUI.backgroundColor = EasyMaterialSettings.ErrorTint();
            } else if (materialData == _selected) {
                GUI.backgroundColor = EasyMaterialSettings.SelectedTint();
            } else if (dragTarget) {
                GUI.backgroundColor = Color.green;
            } else {
                GUI.backgroundColor = materialData.GetColor();
            }

            if (GUI.Button(rect, content, style)) {
                // if left mouse button was clicked
                if (Event.current.button == 0) {
                    // ignore the next selection change callback to prevent the clicked material being moved to the top of the list
                    _ignoreNextSelectionChange = true;

                    // make the editor select and highlight the material in all (non-locked) open project windows
                    materialData.Select();
                }

                _selected = materialData;
                Repaint();
            }

            GUI.backgroundColor = previousBackgroundColor;
        }

        private void DrawStatusBar(Event e) {
            // set area position and width so its not rubbing against the sides
            float x = 4.0f;
            float y = position.height - StatusBarHeight;
            float statusBarWidth = (position.width - (x * 2.0f));
            Rect statusBarRect = new(x, y, statusBarWidth, StatusBarHeight);

            using (new GUILayout.AreaScope(statusBarRect)) {
                using (new HorizontalScope()) {
                    float materialCountWidth = 0.0f;
                    float sliderWidth = 64.0f;
                    float rightTextWidth = 28.0f;

                    if (EasyMaterialSettings.MaterialCountStatusBar()) {
                        materialCountWidth = 28.0f;
                        GUILayout.Label($"[{_materials.Count}]", GUILayout.MinWidth(16.0f), GUILayout.MaxWidth(materialCountWidth));
                    }

                    float availableWidth = (statusBarWidth - (materialCountWidth + sliderWidth));

                    if (availableWidth > 10.0f) {
                        GUILayout.Label(_statusText, GUILayout.MinWidth(0.0f));
                    } else {
                        GUILayout.FlexibleSpace();
                    }

                    if (_statusTextRight != null && availableWidth > (rightTextWidth + materialCountWidth)) {
                        GUILayout.Label(_statusTextRight, GUILayout.Width(rightTextWidth));
                    }

                    // show horizontal slider for icon size
                    EventType eventType = e.type;
                    int oldIconSize = EasyMaterialSettings.IconSize();
                    float min = EasyMaterialSettings.IconSizeMin;
                    float max = EasyMaterialSettings.IconSizeMax;
                    int newIconSize = (int)GUILayout.HorizontalSlider(oldIconSize, min, max, GUILayout.Width(sliderWidth), GUILayout.Height(StatusBarHeight));

                    // check if icon size has been changed, if so, store new value in prefs
                    if (newIconSize != oldIconSize) {
                        EasyMaterialSettings.IconSize(newIconSize);
                    }

                    // if the slider used the event, set the status bar text depending on which event was used
                    if (e.type == EventType.Used) {
                        switch (eventType) {
                            case EventType.MouseDown:
                            case EventType.MouseDrag: {
                                _statusTextRight = newIconSize.ToString();
                                break;
                            }

                            case EventType.MouseUp:
                            case EventType.Ignore: {
                                _statusTextRight = null;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool AddMaterials(Transform transform, Color color) {
            bool addedMaterials = false;

            if (transform.TryGetComponent(out Renderer renderer)) {
                Material[] sharedMaterials = renderer.sharedMaterials;

                if (sharedMaterials != null && sharedMaterials.Length > 0) {
                    foreach (Material material in sharedMaterials) {
                        // if material isn't set or is missing then it will be null, skip it
                        if (material != null) {
                            AddMaterial(material, color);
                            addedMaterials |= true;
                        }
                    }
                }
            }

            return addedMaterials;
        }

        private void AddMaterial(Material material, Color color, bool ignoreLimit = false) {
            MaterialData materialData = new(material, color);

            // the MaterialData.Equals() method is overridden which allows these comparisons to work
            if (!_materials.Contains(materialData)) {
                _materials.Add(materialData);
                materialData.Refresh();
            } else if (_materials.GetLast() != materialData) {
                _materials.MoveToLast(materialData);
                materialData.Refresh();
            }

            if (!ignoreLimit) {
                // ensure list size doesn't exceed the settings value
                int maxCount = EasyMaterialSettings.MaxRecentMaterials();
                while (_materials.Count > maxCount && _materials.Count > 0) {
                    _materials.RemoveFirst();
                }
            }

            // set the status bar text to the name of the material
            _statusText = materialData.GetName();
        }

        private void LoadMaterialsFromAsset(string path) {
            Log.d(LogLevel, this, $"LoadMaterialsFromAsset(), path: {path}");

            MaterialBundle materialBundle = AssetDatabase.LoadAssetAtPath<MaterialBundle>(path);

            if (materialBundle != null) {
                // use the first group tint color when loading a single list
                _tintIndex = 0;
                Color color = GetMaterialGroupTint();

                // clear existing materials before loading the new list
                ContextClearList();
                int count = 0;
                LoadMaterialsFromAsset(materialBundle, color, ref count);

                if (count > 0) {
                    string logMsg = ("Loaded " + count + " materials.");

                    // handle auto-lock
                    if (!_locked) {
                        _locked = true;
                        logMsg += " Note: Window is now locked.";
                    }

                    Log.i(LogLevel, this, logMsg);
                }
            } else {
                Log.e(LogLevel, this, $"Invalid asset file: {path}");
            }
        }

        private void LoadMaterialsFromAssets(MaterialBundle[] materialBundles, bool append) {
            // if we're not appending then clear the list and ensure we start with the first tint color
            if (!append) {
                ContextClearList();
                _tintIndex = 0;
            }

            int count = 0;
            foreach (MaterialBundle materialBundle in materialBundles) {
                Color color = GetMaterialGroupTint();
                LoadMaterialsFromAsset(materialBundle, color, ref count);
            }

            if (count > 0) {
                string logMsg;
                if (materialBundles.Length == 1) {
                    logMsg = ("Loaded " + count + " materials.");
                } else {
                    logMsg = ("Loaded " + count + " materials from " + materialBundles.Length + " assets.");
                }

                // handle auto-lock
                if (!_locked) {
                    _locked = true;
                    logMsg += " Note: Window is now locked.";
                }

                Log.i(LogLevel, this, logMsg);
            }
        }

        private void LoadMaterialsFromAsset(MaterialBundle materialBundle, Color color, ref int count) {
            if (materialBundle.Count() > 0) {
                foreach (Material material in materialBundle) {
                    if (material != null) {
                        // add each material from the asset, ignoring the limit set in the preferences
                        AddMaterial(material, color, true);
                        ++count;
                    } else {
                        Log.w(LogLevel, this, "Skipped loading missing material");
                    }
                }
            } else {
                Log.e(LogLevel, this, "No materials found in asset");
            }
        }

        private void SaveMaterialsToAsset(string path) {
            Log.d(LogLevel, this, $"SaveMaterialsToAsset(), path: {path}");

            try {
                MaterialBundle materialBundle = ScriptableObject.CreateInstance<MaterialBundle>();
                foreach (MaterialData materialData in _materials.List()) {
                    materialData.AddToBundle(materialBundle);
                }

                AssetDatabase.CreateAsset(materialBundle, path);
                AssetDatabase.SaveAssets();
            } catch (UnityException e) {
                Log.e(LogLevel, this, $"Failed to save materials to: {path}\n{e}");
            }
        }

        private Color GetMaterialGroupTint() {
            // get next color but if it's the same as the most recently used color, go next
            Color color = NextColor();

            if (_materials.Count > 0 && color == _materials.GetLast().GetColor()) {
                NextColor();
            }

            return color;
        }

        private Color NextColor() {
            Color[] tints = EasyMaterialSettings.MaterialGroupTints();
            Color color = tints[_tintIndex];
            _tintIndex = (++_tintIndex % tints.Length);
            return color;
        }
    }
}
