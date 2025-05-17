using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [VolumeComponentMenu("Nexcide/Static Noise")]
    public class StaticNoise : VolumeComponentBase {

        public ClampedFloatParameter Blend = new(0.0f, 0.0f, 1.0f);
        public NoInterpColorParameter StaticColor = new(Color.white, hdr: true, showAlpha: false, showEyeDropper: false);
        public NoInterpFloatParameter StaticSize = new(1.0f);
        public NoInterpFloatParameter StaticMin = new(0.0f);
        public NoInterpClampedFloatParameter StaticHz = new(4.0f, 0.0f, 60.0f);
        public NoInterpColorParameter LinesColor = new(Color.white, hdr: true, showAlpha: false, showEyeDropper: false);
        public NoInterpFloatParameter LinesScale = new(100.0f);
        public NoInterpFloatParameter LinesMin = new(0.0f);
        public NoInterpFloatParameter LinesSpeed = new(1.0f);
        public NoInterpClampedFloatParameter LinesHz = new(4.0f, 0.0f, 60.0f);

        public override bool IsActive() => (Blend.value > 0.0f);
    }

    [PostProcessEffect(typeof(StaticNoise))]
    public class StaticNoiseEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Static Noise";

        private static readonly int _blend = Shader.PropertyToID("_Blend");
        private static readonly int _staticColor = Shader.PropertyToID("_StaticColor");
        private static readonly int _staticSize = Shader.PropertyToID("_Static_Size");
        private static readonly int _staticMin = Shader.PropertyToID("_Static_Min");
        private static readonly int _staticHz = Shader.PropertyToID("_StaticHz");
        private static readonly int _linesColor = Shader.PropertyToID("_Lines_Color");
        private static readonly int _linesScale = Shader.PropertyToID("_Lines_Scale");
        private static readonly int _linesMin = Shader.PropertyToID("_Lines_Min");
        private static readonly int _linesSpeed = Shader.PropertyToID("_Lines_Speed");
        private static readonly int _linesHz = Shader.PropertyToID("_Lines_Hz");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out StaticNoise component, out material);

            if (active) {
                material.SetFloat(_blend, component.Blend.value);
                material.SetColor(_staticColor, component.StaticColor.value);
                material.SetFloat(_staticSize, component.StaticSize.value);
                material.SetFloat(_staticMin, component.StaticMin.value);
                material.SetFloat(_staticHz, component.StaticHz.value);
                material.SetColor(_linesColor, component.LinesColor.value);
                material.SetFloat(_linesScale, component.LinesScale.value);
                material.SetFloat(_linesMin, component.LinesMin.value);
                material.SetFloat(_linesSpeed, component.LinesSpeed.value);
                material.SetFloat(_linesHz, component.LinesHz.value);
            }

            return active;
        }
    }
}
