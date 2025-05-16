using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    public abstract class VolumeComponentBase : VolumeComponent, IPostProcessComponent {

        public abstract bool IsActive();

        public bool IsTileCompatible() => false;
    }

    public abstract class VolumeEffect {

        private Material _material;

        public abstract string ShaderName { get; }

        public abstract bool ConfigureMaterial(VolumeStack volumeStack, out Material material);

        protected bool ComponentActive<T>(VolumeStack volumeStack, out T component, out Material material) where T : VolumeComponentBase {
            material = null;
            component = volumeStack.GetComponent<T>();
            return (component.IsActive() && GetMaterial(out material));
        }

        private bool GetMaterial(out Material material) {
            if (_material == null) {
                Shader shader = Shader.Find(ShaderName);

                if (shader != null) {
                    _material = new Material(shader);
                } else {
                    Log.e($"Couldn't find shader: {ShaderName}");
                }
            }

            material = _material;

            return (material != null);
        }
    }
}
