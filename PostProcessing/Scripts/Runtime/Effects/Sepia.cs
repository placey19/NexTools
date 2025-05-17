using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [VolumeComponentMenu("Nexcide/Sepia")]
    public class Sepia : VolumeComponentBase {

        public ClampedFloatParameter Blend = new(0.0f, 0.0f, 1.0f, false);
        public ClampedFloatParameter Exposure = new(1.0f, 0.0f, 10.0f, false);

        public override bool IsActive() => (Blend.value > 0.0f || Exposure != 1.0f);
    }

    [PostProcessEffect(typeof(Sepia))]
    public class SepiaEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Sepia";

        private static readonly int _blend = Shader.PropertyToID("_Blend");
        private static readonly int _exposure = Shader.PropertyToID("_Exposure");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out Sepia component, out material);

            if (active) {
                material.SetFloat(_blend, component.Blend.value);
                material.SetFloat(_exposure, component.Exposure.value);
            }

            return active;
        }
    }
}
