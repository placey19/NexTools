using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nexcide.PostProcessing {

    [CustomEditor(typeof(ShaderList))]
    public class ShaderListEditor : Editor {

        public override void OnInspectorGUI() {
            if (GUILayout.Button("Clear & Add All", GUILayout.ExpandWidth(false))) {
                AddAll();
            }

            DrawDefaultInspector();
        }

        private void AddAll() {
            ShaderList shaderList = (ShaderList)target;
            Undo.RecordObject(shaderList, "Clear & Add All");

            shaderList.Shaders.Clear();

            List<VolumeEffect> effects = ShaderList.CreateAllVolumeEffects();

            foreach (VolumeEffect effect in effects) {
                string shaderName = effect.ShaderName;
                Shader shader = Shader.Find(shaderName);

                if (shader != null) {
                    if (AssetDatabase.IsMainAsset(shader)) {
                        shaderList.Shaders.Add(shader);
                    } else {
                        Log.e(this, $"Invalid shader asset: {shaderName}");
                    }
                } else {
                    Log.e(this, $"Couldn't find shader: {shaderName}");
                }
            }

            EditorUtility.SetDirty(shaderList);
        }
    }
}
