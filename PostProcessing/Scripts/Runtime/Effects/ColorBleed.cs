using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [Serializable, VolumeComponentMenu("Nexcide/Color Bleed")]
    public class ColorBleed : VolumeComponentBase {

        public FloatParameter Blend = new ClampedFloatParameter(0.0f, 0.0f, 1.0f, false);
        public NoInterpClampedFloatParameter Iterations = new NoInterpClampedFloatParameter(10.0f, 0.0f, 20.0f);
        public NoInterpClampedFloatParameter MinAmount = new NoInterpClampedFloatParameter(1.2f, -20.0f, 20.0f);
        public NoInterpClampedFloatParameter MaxAmount = new NoInterpClampedFloatParameter(1.2f, -20.0f, 20.0f);
        public NoInterpClampedFloatParameter ChromaticAberration = new NoInterpClampedFloatParameter(3.0f, -20.0f, 20.0f);

        public override bool IsActive() => (Blend.value > 0.0f);
    }

    [PostProcessEffect(typeof(ColorBleed))]
    public class ColorBleedEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Color Bleed";

        private readonly int _blend = Shader.PropertyToID("_Blend");
        private readonly int _iterations = Shader.PropertyToID("_Iterations");
        private readonly int _minAmount = Shader.PropertyToID("_MinAmount");
        private readonly int _maxAmount = Shader.PropertyToID("_MaxAmount");
        private readonly int _chromaticAberration = Shader.PropertyToID("_ChromaticAberration");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out ColorBleed component, out material);

            if (active) {
                material.SetFloat(_blend, component.Blend.value);
                material.SetFloat(_iterations, component.Iterations.value);
                material.SetFloat(_minAmount, component.MinAmount.value);
                material.SetFloat(_maxAmount, component.MaxAmount.value);
                material.SetFloat(_chromaticAberration, component.ChromaticAberration.value);
            }

            return active;
        }
    }
}
