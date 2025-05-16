using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [Serializable, VolumeComponentMenu("Nexcide/RGB Lines")]
    public class RGBLines : VolumeComponentBase {

        public FloatParameter Blend = new ClampedFloatParameter(0.0f, 0.0f, 1.0f, false);
        public FloatParameter Exposure = new ClampedFloatParameter(1.0f, 0.0f, 10.0f, false);
        public Vector2Parameter Offset = new Vector2Parameter(new Vector2(0.0f, 0.0f), false);
        public Vector2Parameter RGBSplit = new Vector2Parameter(new Vector2(0.005f, 0.005f), false);
        public FloatParameter Blur = new ClampedFloatParameter(0.0f, 0.0f, 1.0f, false);
        public NoInterpClampedFloatParameter Speed = new NoInterpClampedFloatParameter(0.02f, 0.0f, 1.0f, false);

        public override bool IsActive() => (Blend.value > 0.0f || Exposure != 1.0f);
    }

    [PostProcessEffect(typeof(RGBLines))]
    public class RGBLinesEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/RGB Lines";

        private readonly int _blend = Shader.PropertyToID("_Blend");
        private readonly int _exposure = Shader.PropertyToID("_Exposure");
        private readonly int _offset = Shader.PropertyToID("_Offset");
        private readonly int _rgbSplit = Shader.PropertyToID("_RGBSplit");
        private readonly int _blur = Shader.PropertyToID("_Blur");
        private readonly int _speed = Shader.PropertyToID("_Speed");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out RGBLines component, out material);

            if (active) {
                material.SetFloat(_blend, component.Blend.value);
                material.SetFloat(_exposure, component.Exposure.value);
                material.SetVector(_offset, component.Offset.value);
                material.SetVector(_rgbSplit, component.RGBSplit.value);
                material.SetFloat(_blur, component.Blur.value);
                material.SetFloat(_speed, component.Speed.value);
            }

            return active;
        }
    }
}
