using UnityEditor;
using UnityEngine;

using static Nexcide.EditorUtil;

namespace Nexcide.TileKing {

    [CustomEditor(typeof(TileKingMesh))]
    public class TileKingMeshEditor : Editor {

        public LogLevel LogLevel => TileKingSettings.GetLogLevel();

        [MenuItem("GameObject/Nexcide/Tile King Mesh")]
        public static void CreateTileKingMesh() {
            GameObject prefabAsset = TileKingSettings.GetPrefabAsset();
            SpawnPrefab(prefabAsset, "Spawn Tile King Mesh", Vector3.zero, PrefabUnpack.OutermostRoot);
        }

        private string _materialProperty = "_Tiling";

        public override void OnInspectorGUI() {
            TileKingMesh tileKingMesh = (TileKingMesh)target;
            bool generate = false;

            // ensure instances of the core assets are always available
            SerializedProperty canvasProperty = serializedObject.FindProperty(nameof(TileKingMesh.Canvas));
            SerializedProperty meshAssetProperty = serializedObject.FindProperty(nameof(TileKingMesh.MeshAsset));
            TileCanvas canvas = EnsureCoreAssetsAvailable(canvasProperty, meshAssetProperty, ref generate);

            AddHeader("Assets");
            EditorGUILayout.PropertyField(canvasProperty);
            EditorGUILayout.PropertyField(meshAssetProperty);
            EditorGUILayout.Space();

            AddHeader("Core Settings");
            DrawCanvasSize(canvas, ref generate);
            DrawTilesSize(tileKingMesh, canvas, ref generate);
            DrawCanvasMaterials(tileKingMesh, ref generate);

            AddHeader("Color Settings");
            DrawHueRange(canvas, ref generate);
            DrawSaturationRange(canvas, ref generate);
            DrawValueRange(canvas, ref generate);

            AddHeader("Randomization");
            DrawRandomizeMaterialsButton(tileKingMesh, ref generate);
            DrawRandomizeColorsButton(canvas, ref generate);
            DrawRandomizeRotationButton(canvas, ref generate);
            DrawRandomizeUVsButton(canvas, ref generate);

            EditorGUILayout.Space();
            DrawGenerateButton(ref generate);

            serializedObject.ApplyModifiedProperties();

            if (generate) {
                EditorUtility.SetDirty(canvas);
                EditorUtility.SetDirty(tileKingMesh);
                GenerateMesh(tileKingMesh);
            }
        }

        private void DrawCanvasSize(TileCanvas canvas, ref bool generate) {
            EditorGUILayout.LabelField("Canvas Size");

            int canvasWidth = canvas.Width();
            int canvasHeight = canvas.Height();
            int oldCanvasWidth = canvasWidth;
            int oldCanvasHeight = canvasHeight;

            using (new GUILayout.HorizontalScope()) {
                TextFieldInt(canvasWidth, out canvasWidth);
                TextFieldInt(canvasHeight, out canvasHeight);
            }
            EditorGUILayout.Space();

            // if canvas size has changed
            if (canvasWidth != oldCanvasWidth || canvasHeight != oldCanvasHeight) {
                // ignore new values if canvas area is greater than 256^2
                if (canvasWidth * canvasHeight <= 65536) {
                    canvas.Resize(canvasWidth, canvasHeight);
                    generate = true;
                }
            }
        }

        private void DrawCanvasMaterials(TileKingMesh tileKingMesh, ref bool generate) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TileKingMesh.Materials)));
            EditorGUILayout.Space();

            if (serializedObject.hasModifiedProperties) {
                generate = true;
            }
        }

        private void DrawTilesSize(TileKingMesh tileKingMesh, TileCanvas canvas, ref bool generate) {
            EditorGUILayout.LabelField("Tile Size");

            Vector2 tileSize = canvas.TileSize();
            Vector2 oldTileSize = tileSize;

            using (new GUILayout.HorizontalScope()) {
                TextFieldFloat(oldTileSize.x, out tileSize.x);
                TextFieldFloat(oldTileSize.y, out tileSize.y);
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Material Scale");

            Vector2 materialScale = canvas.MaterialScale();
            Vector2 oldMaterialScale = materialScale;

            using (new GUILayout.HorizontalScope()) {
                TextFieldFloat(oldMaterialScale.x, out materialScale.x);
                TextFieldFloat(oldMaterialScale.y, out materialScale.y);
            }

            using (new GUILayout.HorizontalScope()) {
                if (GUILayout.Button("Read scale from material element 0")) {
                    Material material = tileKingMesh.Materials[0];

                    if (material.shader.name.EndsWith("Lit")) {
                        materialScale = material.GetTextureScale("_BaseMap");
                    } else if (material.HasProperty(_materialProperty)) {
                        materialScale = material.GetVector(_materialProperty);
                    } else {
                        Log.e(LogLevel, this, "Material isn't default URP Lit, and doesn't have property: " + _materialProperty);
                    }
                }

                _materialProperty = EditorGUILayout.TextField(_materialProperty);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("All materials must use the same scale", MessageType.Info);
            EditorGUILayout.Space();

            // if tile size or material scale has changed
            if (tileSize != oldTileSize || materialScale != oldMaterialScale) {
                canvas.SetSizeAndScale(tileSize, materialScale);
                generate = true;
            }
        }

        private void DrawHueRange(TileCanvas canvas, ref bool generate) {
            EditorGUILayout.LabelField("Hue Range");

            Color oldFromColor = canvas.FromColor;
            Color oldToColor = canvas.ToColor;

            using (new GUILayout.HorizontalScope()) {
                canvas.FromColor = EditorGUILayout.ColorField(canvas.FromColor);
                canvas.ToColor = EditorGUILayout.ColorField(canvas.ToColor);
            }
            EditorGUILayout.Space();

            // if color changed
            if (oldFromColor != canvas.FromColor || oldToColor != canvas.ToColor) {
                canvas.RefreshColors();
                generate = true;
            }
        }

        private void DrawSaturationRange(TileCanvas canvas, ref bool generate) {
            float oldSaturationMin = canvas.SaturationMin;
            float oldSaturationMax = canvas.SaturationMax;

            EditorGUILayout.LabelField("Saturation Range");
            Undo.RecordObject(canvas, "Saturation Range");

            using (new GUILayout.HorizontalScope()) {
                TextFieldFloat(canvas.SaturationMin, out canvas.SaturationMin);
                TextFieldFloat(canvas.SaturationMax, out canvas.SaturationMax);
            }
            EditorGUILayout.MinMaxSlider(ref canvas.SaturationMin, ref canvas.SaturationMax, 0.0f, 1.0f);
            EditorGUILayout.Space();

            bool saturationMinChanged = (canvas.SaturationMin != oldSaturationMin);
            bool saturationMaxChanged = (canvas.SaturationMax != oldSaturationMax);

            // validate saturation
            if (saturationMinChanged || saturationMaxChanged) {
                if (saturationMinChanged) {
                    canvas.SaturationMin = Mathf.Clamp01(canvas.SaturationMin);
                    canvas.SaturationMax = (canvas.SaturationMin > canvas.SaturationMax ? canvas.SaturationMin : canvas.SaturationMax);
                }

                if (saturationMaxChanged) {
                    canvas.SaturationMax = Mathf.Clamp01(canvas.SaturationMax);
                    canvas.SaturationMin = (canvas.SaturationMax < canvas.SaturationMin ? canvas.SaturationMax : canvas.SaturationMin);
                }

                canvas.RefreshColors();
                generate = true;
            }
        }

        private void DrawValueRange(TileCanvas canvas, ref bool generate) {
            float oldValueMin = canvas.ValueMin;
            float oldValueMax = canvas.ValueMax;

            EditorGUILayout.LabelField("Value Range");
            Undo.RecordObject(canvas, "Value Range");

            using (new GUILayout.HorizontalScope()) {
                TextFieldFloat(canvas.ValueMin, out canvas.ValueMin);
                TextFieldFloat(canvas.ValueMax, out canvas.ValueMax);
            }
            EditorGUILayout.MinMaxSlider(ref canvas.ValueMin, ref canvas.ValueMax, 0.0f, 1.0f);
            EditorGUILayout.Space();

            bool valueMinChanged = (canvas.ValueMin != oldValueMin);
            bool valueMaxChanged = (canvas.ValueMax != oldValueMax);

            // validate values
            if (valueMinChanged || valueMaxChanged) {
                if (valueMinChanged) {
                    canvas.ValueMin = Mathf.Clamp01(canvas.ValueMin);
                    canvas.ValueMax = (canvas.ValueMin > canvas.ValueMax ? canvas.ValueMin : canvas.ValueMax);
                }

                if (valueMaxChanged) {
                    canvas.ValueMax = Mathf.Clamp01(canvas.ValueMax);
                    canvas.ValueMin = (canvas.ValueMax < canvas.ValueMin ? canvas.ValueMax : canvas.ValueMin);
                }

                canvas.RefreshColors();
                generate = true;
            }
        }

        private void DrawRandomizeMaterialsButton(TileKingMesh tileKingMesh, ref bool generate) {
            using (new GUILayout.HorizontalScope()) {
                if (GUILayout.Button("Randomize Materials")) {
                    System.Random random = new System.Random((int)(Time.realtimeSinceStartup * 1000.0f));
                    int materialsCount = tileKingMesh.Materials.Count;
                    tileKingMesh.Canvas.RandomizeMaterials(seed: random.Next(), materialsCount);
                    generate = true;
                }

                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) {
                    tileKingMesh.Canvas.ResetMaterials();
                    generate = true;
                }
            }
        }

        private void DrawRandomizeColorsButton(TileCanvas canvas, ref bool generate) {
            using (new GUILayout.HorizontalScope()) {
                if (GUILayout.Button("Randomize Colors")) {
                    System.Random random = new System.Random((int)(Time.realtimeSinceStartup * 1000.0f));
                    canvas.RandomizeColors(seed: random.Next());
                    generate = true;
                }

                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) {
                    canvas.ResetColors(Color.white);
                    generate = true;
                }
            }
        }

        private void DrawRandomizeRotationButton(TileCanvas canvas, ref bool generate) {
            using (new GUILayout.HorizontalScope()) {
                if (GUILayout.Button("Randomize Rotation")) {
                    System.Random random = new System.Random((int)(Time.realtimeSinceStartup * 1000.0f));
                    canvas.RandomizeRotations(seed: random.Next());
                    generate = true;
                }

                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) {
                    canvas.ResetRotations();
                    generate = true;
                }
            }
        }

        private void DrawRandomizeUVsButton(TileCanvas canvas, ref bool generate) {
            using (new GUILayout.HorizontalScope()) {
                if (GUILayout.Button("Randomize UVs")) {
                    System.Random random = new System.Random((int)(Time.realtimeSinceStartup * 1000.0f));
                    canvas.RandomizeUVs(seed: random.Next());
                    generate = true;
                }

                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) {
                    canvas.ResetUVs();
                    generate = true;
                }
            }
        }

        private void DrawGenerateButton(ref bool generate) {
            if (GUILayout.Button("Generate")) {
                generate = true;
            }
        }

        private TileCanvas EnsureCoreAssetsAvailable(SerializedProperty modelProperty, SerializedProperty meshAssetProperty, ref bool generate) {
            // make sure a canvas instance is always available
            TileCanvas canvas = (TileCanvas)modelProperty.objectReferenceValue;
            if (canvas == null) {
                canvas = ScriptableObject.CreateInstance<TileCanvas>();
                modelProperty.objectReferenceValue = canvas;
                generate = true;
            }

            // make sure a mesh instance is always available
            Mesh mesh = (Mesh)meshAssetProperty.objectReferenceValue;
            if (mesh == null) {
                mesh = new Mesh {
                    name = " (Fresh Mesh)"
                };

                meshAssetProperty.objectReferenceValue = mesh;
                generate = true;
            }

            return canvas;
        }

        private void GenerateMesh(TileKingMesh tileKingMesh) {
            if (tileKingMesh.MeshAsset != null && tileKingMesh.Canvas != null) {
                TileKingMeshCreator creator = new();
                creator.PopulateMesh(tileKingMesh);

                EditorUtility.SetDirty(tileKingMesh.MeshAsset);

                if (tileKingMesh.TryGetComponent(out MeshFilter meshFilter)) {
                    meshFilter.sharedMesh = tileKingMesh.MeshAsset;
                }

                Vector2 tileSize = tileKingMesh.Canvas.TileSize();
                if (tileSize.x > 0.0f && tileSize.y > 0.0f && tileKingMesh.MeshAsset.vertexCount >= 3) {
                    if (tileKingMesh.TryGetComponent(out MeshCollider meshCollider)) {
                        meshCollider.sharedMesh = tileKingMesh.MeshAsset;
                    }
                }

                if (tileKingMesh.Materials != null) {
                    if (tileKingMesh.TryGetComponent(out MeshRenderer meshRenderer)) {
                        meshRenderer.sharedMaterials = tileKingMesh.Materials.ToArray();
                    }
                }
            }
        }
    }
}
