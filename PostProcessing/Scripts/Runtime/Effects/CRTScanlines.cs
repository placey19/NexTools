using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [Serializable, VolumeComponentMenu("Nexcide/CRT Scanlines")]
    public class CRTScanlines : VolumeComponentBase {

        public FloatParameter Opacity = new ClampedFloatParameter(0.0f, 0.0f, 1.0f, false);
        public NoInterpFloatParameter Scale = new NoInterpFloatParameter(1.0f, false);
        public FloatParameter ColorBleed = new ClampedFloatParameter(0.0f, 0.0f, 5.0f, false);

        public override bool IsActive() => (Opacity.value > 0.0f);
    }

    [PostProcessEffect(typeof(CRTScanlines))]
    public class CRTScanlinesEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/CRT Scanlines";

        private readonly int _opacity = Shader.PropertyToID("_Opacity");
        private readonly int _scale = Shader.PropertyToID("_Scale");
        private readonly int _colorBleed = Shader.PropertyToID("_ColorBleed");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out CRTScanlines component, out material);

            if (active) {
                material.SetFloat(_opacity, component.Opacity.value);
                material.SetFloat(_scale, component.Scale.value);
                material.SetFloat(_colorBleed, component.ColorBleed.value);
            }

            return active;
        }
    }
}
