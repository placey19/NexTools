using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [Serializable, VolumeComponentMenu("Nexcide/Grayscale")]
    public class Grayscale : VolumeComponentBase {

        public FloatParameter Blend = new ClampedFloatParameter(0.0f, 0.0f, 1.0f, false);
        public FloatParameter Exposure = new ClampedFloatParameter(1.0f, 0.0f, 10.0f, false);
        public BoolParameter Invert = new BoolParameter(false, false);

        public override bool IsActive() => (Blend.value > 0.0f || Exposure != 1.0f);
    }

    [PostProcessEffect(typeof(Grayscale))]
    public class GrayscaleEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Grayscale";

        private readonly int _blend = Shader.PropertyToID("_Blend");
        private readonly int _exposure = Shader.PropertyToID("_Exposure");
        private readonly int _invert = Shader.PropertyToID("_Invert");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out Grayscale component, out material);

            if (active) {
                material.SetFloat(_blend, component.Blend.value);
                material.SetFloat(_exposure, component.Exposure.value);
                material.SetFloat(_invert, component.Invert.value ? 1.0f : 0.0f);
            }

            return active;
        }
    }
}
