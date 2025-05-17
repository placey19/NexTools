using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [VolumeComponentMenu("Nexcide/RGB Lines")]
    public class RGBLines : VolumeComponentBase {

        public ClampedFloatParameter Blend = new(0.0f, 0.0f, 1.0f);
        public ClampedFloatParameter Exposure = new(1.0f, 0.0f, 10.0f);
        public Vector2Parameter Offset = new(new Vector2(0.0f, 0.0f));
        public Vector2Parameter RGBSplit = new(new Vector2(0.005f, 0.005f));
        public ClampedFloatParameter Blur = new(0.0f, 0.0f, 1.0f);
        public NoInterpClampedFloatParameter Speed = new(0.02f, 0.0f, 1.0f);

        public override bool IsActive() => (Blend.value > 0.0f || Exposure != 1.0f);
    }

    [PostProcessEffect(typeof(RGBLines))]
    public class RGBLinesEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/RGB Lines";

        private static readonly int _blend = Shader.PropertyToID("_Blend");
        private static readonly int _exposure = Shader.PropertyToID("_Exposure");
        private static readonly int _offset = Shader.PropertyToID("_Offset");
        private static readonly int _rgbSplit = Shader.PropertyToID("_RGBSplit");
        private static readonly int _blur = Shader.PropertyToID("_Blur");
        private static readonly int _speed = Shader.PropertyToID("_Speed");

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
