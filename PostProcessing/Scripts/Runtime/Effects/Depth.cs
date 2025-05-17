using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [VolumeComponentMenu("Nexcide/Depth")]
    public class Depth : VolumeComponentBase {

        public ClampedFloatParameter Blend = new(0.0f, 0.0f, 1.0f);
        public NoInterpColorParameter NearColor = new(Color.blue);
        public NoInterpColorParameter FarColor = new(Color.black);
        public NoInterpClampedFloatParameter Min = new(5.0f, 0.0f, 1000.0f);
        public NoInterpClampedFloatParameter Max = new(50.0f, 0.0f, 1000.0f);

        public override bool IsActive() => (Blend.value > 0.0f);
    }

    [PostProcessEffect(typeof(Depth))]
    public class DepthEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Depth";

        private static readonly int _blend = Shader.PropertyToID("_Blend");
        private static readonly int _nearColor = Shader.PropertyToID("_NearColor");
        private static readonly int _farColor = Shader.PropertyToID("_FarColor");
        private static readonly int _min = Shader.PropertyToID("_Min");
        private static readonly int _max = Shader.PropertyToID("_Max");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out Depth component, out material);

            if (active) {
                material.SetFloat(_blend, component.Blend.value);
                material.SetColor(_nearColor, component.NearColor.value);
                material.SetColor(_farColor, component.FarColor.value);
                material.SetFloat(_min, component.Min.value);
                material.SetFloat(_max, component.Max.value);
            }

            return active;
        }
    }
}
