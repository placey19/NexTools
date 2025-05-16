using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [Serializable, VolumeComponentMenu("Nexcide/Depth")]
    public class Depth : VolumeComponentBase {

        public FloatParameter Blend = new ClampedFloatParameter(0.0f, 0.0f, 1.0f, false);
        public NoInterpColorParameter NearColor = new NoInterpColorParameter(Color.blue, false);
        public NoInterpColorParameter FarColor = new NoInterpColorParameter(Color.black, false);
        public NoInterpClampedFloatParameter Min = new NoInterpClampedFloatParameter(5.0f, 0.0f, 1000.0f, false);
        public NoInterpClampedFloatParameter Max = new NoInterpClampedFloatParameter(50.0f, 0.0f, 1000.0f, false);

        public override bool IsActive() => (Blend.value > 0.0f);
    }

    [PostProcessEffect(typeof(Depth))]
    public class DepthEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Depth";

        private readonly int _blend = Shader.PropertyToID("_Blend");
        private readonly int _nearColor = Shader.PropertyToID("_NearColor");
        private readonly int _farColor = Shader.PropertyToID("_FarColor");
        private readonly int _min = Shader.PropertyToID("_Min");
        private readonly int _max = Shader.PropertyToID("_Max");

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
