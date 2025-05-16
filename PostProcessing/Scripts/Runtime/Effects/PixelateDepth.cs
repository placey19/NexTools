using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [Serializable, VolumeComponentMenu("Nexcide/Pixelate Depth")]
    public class PixelateDepth : VolumeComponentBase {

        public FloatParameter Blend = new ClampedFloatParameter(0.0f, 0.0f, 1.0f, false);
        public NoInterpFloatParameter PixelSizeNear = new NoInterpFloatParameter(2.0f, false);
        public NoInterpFloatParameter PixelSizeFar = new NoInterpFloatParameter(50.0f, false);
        public NoInterpFloatParameter MaxDistance = new NoInterpFloatParameter(100.0f, false);
        public NoInterpFloatParameter DepthStep = new NoInterpFloatParameter(1.0f, false);
        
        public override bool IsActive() => (Blend.value > 0.0f);
    }

    [PostProcessEffect(typeof(PixelateDepth))]
    public class PixelateDepthEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Pixelate Depth";

        private readonly int _blend = Shader.PropertyToID("_Blend");
        private readonly int _pixelSizeNear = Shader.PropertyToID("_PixelSizeNear");
        private readonly int _pixelSizeFar = Shader.PropertyToID("_PixelSizeFar");
        private readonly int _maxDistance = Shader.PropertyToID("_MaxDistance");
        private readonly int _depthStep = Shader.PropertyToID("_DepthStep");

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
