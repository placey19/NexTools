using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [Serializable, VolumeComponentMenu("Nexcide/Pixelate")]
    public class Pixelate : VolumeComponentBase {

        public FloatParameter PixelSize = new ClampedFloatParameter(0.0f, 0.0f, 100.0f, false);
        public ColorParameter Tint = new ColorParameter(Color.white, false);

        public override bool IsActive() => (PixelSize.value > 0.0f);
    }

    [PostProcessEffect(typeof(Pixelate))]
    public class PixelateEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Pixelate";

        private readonly int _pixelSize = Shader.PropertyToID("_PixelSize");
        private readonly int _tint = Shader.PropertyToID("_Tint");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out Pixelate component, out material);

            if (active) {
                material.SetFloat(_pixelSize, component.PixelSize.value);
                material.SetColor(_tint, component.Tint.value);
            }

            return active;
        }
    }
}
