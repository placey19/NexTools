using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [Serializable, VolumeComponentMenu("Nexcide/Static Noise")]
    public class StaticNoise : VolumeComponentBase {

        public ClampedFloatParameter Blend = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
        public NoInterpColorParameter StaticColor = new NoInterpColorParameter(Color.white, hdr: true, showAlpha: false, showEyeDropper: false);
        public NoInterpFloatParameter StaticSize = new NoInterpFloatParameter(1.0f);
        public NoInterpFloatParameter StaticMin = new NoInterpFloatParameter(0.0f);
        public NoInterpClampedFloatParameter StaticHz = new NoInterpClampedFloatParameter(4.0f, 0.0f, 60.0f);
        public NoInterpColorParameter LinesColor = new NoInterpColorParameter(Color.white, hdr: true, showAlpha: false, showEyeDropper: false);
        public NoInterpFloatParameter LinesScale = new NoInterpFloatParameter(100.0f);
        public NoInterpFloatParameter LinesMin = new NoInterpFloatParameter(0.0f);
        public NoInterpFloatParameter LinesSpeed = new NoInterpFloatParameter(1.0f);
        public NoInterpClampedFloatParameter LinesHz = new NoInterpClampedFloatParameter(4.0f, 0.0f, 60.0f);

        public override bool IsActive() => (Blend.value > 0.0f);
    }

    [PostProcessEffect(typeof(StaticNoise))]
    public class StaticNoiseEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Static Noise";

        private readonly int _blend = Shader.PropertyToID("_Blend");
        private readonly int _staticColor = Shader.PropertyToID("_StaticColor");
        private readonly int _staticSize = Shader.PropertyToID("_Static_Size");
        private readonly int _staticMin = Shader.PropertyToID("_Static_Min");
        private readonly int _staticHz = Shader.PropertyToID("_StaticHz");
        private readonly int _linesColor = Shader.PropertyToID("_Lines_Color");
        private readonly int _linesScale = Shader.PropertyToID("_Lines_Scale");
        private readonly int _linesMin = Shader.PropertyToID("_Lines_Min");
        private readonly int _linesSpeed = Shader.PropertyToID("_Lines_Speed");
        private readonly int _linesHz = Shader.PropertyToID("_Lines_Hz");

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
