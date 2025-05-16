using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [Serializable, VolumeComponentMenu("Nexcide/Color Fade")]
    public class ColorFade : VolumeComponentBase {

        public FloatParameter Amount = new ClampedFloatParameter(0.0f, 0.0f, 1.0f, false);
        public ColorParameter Color = new ColorParameter(UnityEngine.Color.white, hdr: true, showAlpha: false, showEyeDropper: false, false);

        public override bool IsActive() => (Amount.value > 0.0f);
    }

    [PostProcessEffect(typeof(ColorFade))]
    public class ColorFadeEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Color Fade";

        private readonly int _amount = Shader.PropertyToID("_Amount");
        private readonly int _color = Shader.PropertyToID("_Color");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out ColorFade component, out material);

            if (active) {
                material.SetFloat(_amount, component.Amount.value);
                material.SetColor(_color, component.Color.value);
            }

            return active;
        }
    }
}
