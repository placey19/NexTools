using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [VolumeComponentMenu("Nexcide/Color Fade")]
    public class ColorFade : VolumeComponentBase {

        public ClampedFloatParameter Amount = new(0.0f, 0.0f, 1.0f);
        public ColorParameter Color = new(UnityEngine.Color.white, hdr: true, showAlpha: false, showEyeDropper: false);

        public override bool IsActive() => (Amount.value > 0.0f);
    }

    [PostProcessEffect(typeof(ColorFade))]
    public class ColorFadeEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Color Fade";

        private static readonly int _amount = Shader.PropertyToID("_Amount");
        private static readonly int _color = Shader.PropertyToID("_Color");

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
