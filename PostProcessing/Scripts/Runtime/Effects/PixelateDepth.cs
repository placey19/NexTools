using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [VolumeComponentMenu("Nexcide/Pixelate Depth")]
    public class PixelateDepth : VolumeComponentBase {

        public ClampedFloatParameter Blend = new(0.0f, 0.0f, 1.0f);
        public NoInterpFloatParameter PixelSizeNear = new(2.0f);
        public NoInterpFloatParameter PixelSizeFar = new(50.0f);
        public NoInterpFloatParameter MaxDistance = new(100.0f);
        public NoInterpFloatParameter DepthStep = new(1.0f);

        public override bool IsActive() => (Blend.value > 0.0f);
    }

    [PostProcessEffect(typeof(PixelateDepth))]
    public class PixelateDepthEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Pixelate Depth";

        private static readonly int _blend = Shader.PropertyToID("_Blend");
        private static readonly int _pixelSizeNear = Shader.PropertyToID("_PixelSizeNear");
        private static readonly int _pixelSizeFar = Shader.PropertyToID("_PixelSizeFar");
        private static readonly int _maxDistance = Shader.PropertyToID("_MaxDistance");
        private static readonly int _depthStep = Shader.PropertyToID("_DepthStep");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out PixelateDepth component, out material);

            if (active) {
                material.SetFloat(_blend, component.Blend.value);
                material.SetFloat(_pixelSizeNear, component.PixelSizeNear.value);
                material.SetFloat(_pixelSizeFar, component.PixelSizeFar.value);
                material.SetFloat(_maxDistance, component.MaxDistance.value);
                material.SetFloat(_depthStep, component.DepthStep.value);
            }

            return active;
        }
    }
}
