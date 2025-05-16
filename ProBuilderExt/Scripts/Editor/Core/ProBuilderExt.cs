using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;

using static Nexcide.ProBuilderUtil;

namespace Nexcide {

    public class ProBuilderExt : EditorWindow {

        private enum Mode {

            None,
            FacePaint,
            MovePivot,
        }

        private SceneSelection _hovering;
        private bool _rightMouseDown;
        private Color _currentColor = Color.clear;
        private bool _changeColorOnSelect = true;
        private Mode _mode = Mode.None;
        private ProBuilderMesh _mesh;
        private Vector3 _pivotPosition;
        private Vector3 _pivotStartPosition;

        [MenuItem("Nexcide/ProBuilder Ext", false, 34)]
        private static void Init() {
            EditorWindow editorWindow = GetWindow<ProBuilderExt>(typeof(ProBuilderExt));
            editorWindow.titleContent = new GUIContent("ProBuilderExt");
            editorWindow.Show();
        }

        private void OnEnable() {
            SceneView.beforeSceneGui += OnBeforeSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            ProBuilderEditor.selectionUpdated += OnSelectionUpdated;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
        }

        private void OnDisable() {
            SceneView.beforeSceneGui -= OnBeforeSceneGUI;
            SceneView.duringSceneGui -= OnSceneGUI;
            ProBuilderEditor.selectionUpdated -= OnSelectionUpdated;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
        }

        private void OnGUI() {
            using (new GUILayout.VerticalScope()) {
                using (new EditorGUI.DisabledGroupScope(ProBuilderEditor.selectMode != SelectMode.Face)) {
                    EditorGUI.BeginChangeCheck();
                    bool facePaintMode = GUILayout.Toggle(_mode == Mode.FacePaint, "Face Paint Mode");

                    if (EditorGUI.EndChangeCheck()) {
                        SetMode(facePaintMode ? Mode.FacePaint : Mode.None);
                    }

                    _changeColorOnSelect = GUILayout.Toggle(_changeColorOnSelect, "Update Color");
                    _currentColor = EditorGUILayout.ColorField(_currentColor);
                }

                EditorGUILayout.Space();

                using (new EditorGUI.DisabledGroupScope(MeshSelection.selectedObjectCount != 1)) {
                    DrawMovePivot();
                    DrawResetPivot();
                    DrawResetRotation();
                }
            }
        }

        private void DrawResetPivot() {
            if (GUILayout.Button("Reset Pivot")) {
                ResetPivot(MeshSelection.activeMesh);
            }
        }

        private void DrawMovePivot() {
            EditorGUI.BeginChangeCheck();
            bool movePivotMode = GUILayout.Toggle(_mode == Mode.MovePivot, "Move Pivot", GUI.skin.button);

            if (EditorGUI.EndChangeCheck()) {
                if (movePivotMode) {
                    StartPivotMode();
                } else {
                    EndPivotMode();
                }
            }
        }

        private void DrawResetRotation() {
            if (GUILayout.Button("Reset Rotation")) {
                ResetRotation(MeshSelection.activeMesh);
            }
        }

        private void OnBeforeSceneGUI(SceneView sceneView) {
            switch (_mode) {
                case Mode.None:
                case Mode.MovePivot: {
                    // nothing
                    break;
                }

                case Mode.FacePaint: {
                    if (ProBuilderEditor.selectMode == SelectMode.Face) {
                        Event e = Event.current;

                        if (e.type == EventType.MouseMove) {
                            _hovering = _ProBuilderEditor.m_Hovering;
                        }
                    }
                    break;
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView) {
            switch (_mode) {
                case Mode.None: {
                    // nothing
                    break;
                }

                case Mode.MovePivot: {
                    DoMovePivotMode();
                    break;
                }

                case Mode.FacePaint: {
                    DoFacePaintMode();
                    break;
                }
            }
        }

        private void DoFacePaintMode() {
            if (ProBuilderEditor.selectMode == SelectMode.Face) {
                Event e = Event.current;

                if (e.type != EventType.Repaint && e.type != EventType.Layout) {
                    if (_rightMouseDown) {
                        if (e.type == EventType.MouseUp && e.button == 1) {
                            _rightMouseDown = false;

                            OnRightMouseClick();
                        } else if (e.type == EventType.MouseMove) {
                            // prevent SceneView orbit
                            e.Use();
                        }
                    } else if (e.type == EventType.MouseDown && e.button == 1) {
                        _rightMouseDown = true;

                        e.Use();
                    } else {
                        _rightMouseDown = false;
                    }
                }
            }
        }

        private void StartPivotMode() {
            if (MeshSelection.selectedObjectCount == 1) {
                _mesh = MeshSelection.activeMesh;

                if (_mesh != null) {
                    _pivotStartPosition = _mesh.transform.position;
                    _pivotPosition = _pivotStartPosition;
                    SetMode(Mode.MovePivot);
                }
            }
        }

        private void EndPivotMode() {
            if (_mesh != null && _pivotPosition != _pivotStartPosition) {
                MovePivot(_mesh, _pivotStartPosition, _pivotPosition);
            }

            SetMode(Mode.None);
        }

        private void DoMovePivotMode() {
            if (_mesh != null) {
                float size = UnityEditor.HandleUtility.GetHandleSize(_pivotPosition) * 0.25f;

                Handles.color = Color.green;
                Handles.SphereHandleCap(0, _pivotPosition, Quaternion.identity, size, Event.current.type);
                _pivotPosition = Handles.PositionHandle(_pivotPosition, Quaternion.identity);
                _pivotPosition = SnappingUtil.MoveSnap(_pivotPosition);
            } else {
                SetMode(Mode.None);
            }
        }

        private void OnSelectionUpdated(IEnumerable<ProBuilderMesh> meshes) {
            if (_changeColorOnSelect && ProBuilderEditor.selectMode == SelectMode.Face) {
                Event e = Event.current;

                if (e != null && meshes != null) {
                    foreach (ProBuilderMesh mesh in meshes) {
                        if (mesh.selectedFaceCount == 1) {
                            Face[] faces = mesh.GetSelectedFaces();
                            UpdateCurrentColor(mesh, faces[0]);

                            break;
                        }
                    }
                }
            }

            if (_mode == Mode.MovePivot) {
                _EditorUtility.ShowNotification("Move Pivot Cancelled");

                SetMode(Mode.None);
            }

            Repaint();
        }

        private void OnSelectModeChanged(SelectMode selectMode) {
            switch (_mode) {
                case Mode.None: {
                    // nothing
                    break;
                }

                case Mode.MovePivot: {
                    _EditorUtility.ShowNotification("Move Pivot Cancelled");
                    SetMode(Mode.None);
                    Repaint();
                    break;
                }

                case Mode.FacePaint: {
                    _EditorUtility.ShowNotification("Face Paint Cancelled");
                    SetMode(Mode.None);
                    Repaint();
                    break;
                }
            }
        }

        private void OnRightMouseClick() {
            if (ProBuilderEditor.selectMode == SelectMode.Face && _currentColor != Color.clear) {
                if (_hovering != null && _hovering.mesh != null) {
                    List<Face> faces = _hovering.faces;

                    if (faces != null && faces.Count == 1) {
                        ProBuilderMesh mesh = _hovering.mesh;
                        SetFaceColor(mesh, faces, _currentColor.linear);
                    }
                }
            }
        }

        private void SetFaceColor(ProBuilderMesh mesh, List<Face> faces, Color color) {
            Undo.RegisterCompleteObjectUndo(mesh, "Apply Vertex Colors");

            Color[] colors = mesh.GetColors();

            foreach (Face face in faces) {
                foreach (int i in face.distinctIndexes) {
                    colors[i] = color;
                }
            }

            mesh.colors = colors;

            mesh.ToMesh();
            mesh.Refresh();
            mesh.Optimize();
        }

        private void UpdateCurrentColor(ProBuilderMesh mesh, Face face) {
            Color[] colors = mesh.GetColors();

            Color color = Color.clear;
            bool multipleColors = false;

            int previousIndex = -1;
            foreach (int index in face.distinctIndexes) {
                Color thisColor = colors[index];
                color = thisColor;

                if (previousIndex >= 0) {
                    Color previousColor = colors[previousIndex];

                    if (thisColor != previousColor) {
                        multipleColors = true;
                        break;
                    }
                }

                previousIndex = index;
            }

            if (!multipleColors) {
                string colorName = _ColorUtility.GetColorName(color);
                _EditorUtility.ShowNotification("Using Color\n" + colorName);

                // store the color with gamma curve to keep it in sync with ProBuilders color palette
                _currentColor = color.gamma;

                Repaint();
            } else {
                _EditorUtility.ShowNotification("Multiple Vertex Colors on Face");
            }
        }

        private static void ResetPivot(ProBuilderMesh mesh) {
            List<Vector3> positions = new List<Vector3>(mesh.VerticesInWorldSpace());
            Vector3 firstVertex = positions[0];
            MovePivot(mesh, mesh.transform.position, firstVertex);
        }

        private static void MovePivot(ProBuilderMesh mesh, Vector3 startPosition, Vector3 position) {
            const string undoName = "Move Pivot";

            Undo.RegisterCompleteObjectUndo(mesh, undoName);
            Undo.RecordObject(mesh.transform, undoName);

            // remove children
            List<Transform> children = GetChildren(mesh.transform);
            children.ForEach(child => {
                Undo.RecordObject(child, undoName);
                child.parent = null;
            });

            List<Vector3> positions = new List<Vector3>(mesh.VerticesInWorldSpace());
            Vector3 delta = (startPosition - position);

            for (int i = 0; i < positions.Count; ++i) {
                Vector3 vertex = positions[i];
                vertex += delta;
                positions[i] = mesh.transform.InverseTransformPoint(vertex);
            }

            mesh.transform.position = position;
            mesh.positions = positions;

            mesh.ToMesh();
            mesh.Refresh();
            mesh.Optimize();

            // restore children
            children.ForEach(child => child.parent = mesh.transform);

            _EditorUtility.ShowNotification("Pivot Moved");
        }

        private static void ResetRotation(ProBuilderMesh mesh) {
            const string undoName = "Reset Rotation";

            Undo.RegisterCompleteObjectUndo(mesh, undoName);
            Undo.RecordObject(mesh.transform, undoName);

            // remove children
            List<Transform> children = GetChildren(mesh.transform);
            children.ForEach(child => {
                Undo.RecordObject(child, undoName);
                child.parent = null;
            });

            List<Vector3> positions = new List<Vector3>(mesh.VerticesInWorldSpace());
            Vector3 delta = -mesh.transform.position;

            for (int i = 0; i < positions.Count; ++i) {
                Vector3 vertex = positions[i];
                vertex += delta;
                positions[i] = vertex;
            }

            mesh.transform.rotation = Quaternion.identity;
            mesh.positions = positions;

            mesh.ToMesh();
            mesh.Refresh();
            mesh.Optimize();

            // restore children
            children.ForEach(child => child.parent = mesh.transform);

            _EditorUtility.ShowNotification("Reset Rotation");
        }

        private static List<Transform> GetChildren(Transform transform) {
            List<Transform> children = new();

            foreach (Transform child in transform) {
                children.Add(child);
            }

            return children;
        }

        private void SetMode(Mode mode) {
            if (mode != _mode) {
                _mode = mode;

                switch (_mode) {
                    case Mode.None: {
                        _mesh = null;
                        UnityEditor.Tools.hidden = false;
                        break;
                    }

                    case Mode.FacePaint:
                    case Mode.MovePivot: {
                        UnityEditor.Tools.hidden = true;
                        break;
                    }
                }
            }
        }
    }
}
