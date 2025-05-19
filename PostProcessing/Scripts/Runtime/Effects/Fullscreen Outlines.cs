using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [VolumeComponentMenu("Nexcide/Fullscreen Outlines")]
    public class FullscreenOutlines : VolumeComponentBase {

        public ClampedFloatParameter Blend = new(0.0f, 0.0f, 1.0f);
        public ColorParameter OutlineColor = new(Color.black, hdr: true, showAlpha: false, showEyeDropper: false);
        public NoInterpClampedFloatParameter ColorThreshold = new(0.1f, 0.1f, 10.0f);
        public NoInterpClampedFloatParameter NormalThreshold = new(0.5f, 0.001f, 10.0f);
        public NoInterpFloatParameter LineThickness = new(1.0f);

        public override bool IsActive() => (Blend.value > 0.0f);
    }

    [PostProcessEffect(typeof(FullscreenOutlines))]
    public class FullscreenOutlinesEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Fullscreen Outlines";

        private static readonly int _blend = Shader.PropertyToID("_Blend");
        private static readonly int _outlineColor = Shader.PropertyToID("_OutlineColor");
        private static readonly int _colorThreshold = Shader.PropertyToID("_ColorThreshold");
        private static readonly int _normalThreshold = Shader.PropertyToID("_NormalThreshold");
        private static readonly int _lineThickness = Shader.PropertyToID("_LineThickness");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out FullscreenOutlines component, out material);

            if (active) {
                material.SetFloat(_blend, component.Blend.value);
                material.SetColor(_outlineColor, component.OutlineColor.value);
                material.SetFloat(_colorThreshold, component.ColorThreshold.value);
                material.SetFloat(_normalThreshold, component.NormalThreshold.value);
                material.SetFloat(_lineThickness, component.LineThickness.value * 0.001f);
            }

            return active;
        }
    }
}
