using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Nexcide.PostProcessing {

    public sealed class PostProcessEffectsFeature : ScriptableRendererFeature {

        public LogLevel LogLevel;

        [Header("Required")]
        public ShaderList EffectsList;

        [Header("Options")]
        public RenderPassEvent When = RenderPassEvent.AfterRenderingPostProcessing;
        public bool EnableInSceneView = true;

        [Header("Editor Only")]
        [Tooltip("Only shaders referenced in the 'Shaders' list will be included in a build")]
        public bool UseAllEffects;

        private readonly List<PostProcessPass> _passes = new();

#if UNITY_EDITOR
        // check if list of shaders has changed to allow for immediately seeing changes when reordering the list
        private List<Shader> _previousShaders = new();

        private void OnEnable() {
            UnityEditor.EditorApplication.update += Update;
        }

        private void OnDisable() {
            UnityEditor.EditorApplication.update -= Update;
        }

        private void Update() {
            if (EffectsList != null && EffectsList.Shaders != null) {
                if (!Enumerable.SequenceEqual(EffectsList.Shaders, _previousShaders)) {
                    _previousShaders = new(EffectsList.Shaders);

                    Create();
                }
            }
        }
#endif

        public override void Create() {
            if (isActive) {
                _passes.Clear();

                List<VolumeEffect> volumeEffects = null;

                if (!UseAllEffects && EffectsList != null) {
                    volumeEffects = EffectsList.CreateVolumeEffects();
                } else {
                    volumeEffects = ShaderList.CreateAllVolumeEffects();

                    if (!UseAllEffects) {
                        Log.w(LogLevel, this, $"Missing {nameof(ShaderList)}, using all {volumeEffects.Count} found effects");
                    }
                }

                int count = 0;
                if (volumeEffects != null) {
                    foreach (VolumeEffect volumeEffect in volumeEffects) {
                        PostProcessPass pass = new(volumeEffect) {
                            renderPassEvent = When
                        };

                        _passes.Add(pass);

                        Log.v(LogLevel, this, $"Created: {volumeEffect.ShaderName}");
                    }

                    count = volumeEffects.Count;
                }

                Log.d(LogLevel, this, $"Created {volumeEffects.Count} passes");
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            ref CameraData cameraData = ref renderingData.cameraData;
            CameraType cameraType = cameraData.cameraType;

            if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection) {
                return;
            }

            if (cameraType == CameraType.SceneView && !EnableInSceneView) {
                return;
            }

            foreach (PostProcessPass pass in _passes) {
                if (pass.IsEffectActive()) {
                    renderer.EnqueuePass(pass);
                }
            }
        }

        protected override void Dispose(bool disposing) {
            foreach (PostProcessPass pass in _passes) {
                pass.Dispose();
            }

            _passes.Clear();
        }
    }
}
