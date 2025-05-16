using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [Serializable, VolumeComponentMenu("Nexcide/Rectangle")]
    public class Rectangle : VolumeComponentBase {

        public ClampedFloatParameter Opacity = new ClampedFloatParameter(0.0f, 0.0f, 1.0f, false);
        public NoInterpFloatParameter AspectRatio = new NoInterpFloatParameter(1.77777f, false);
        public NoInterpClampedFloatParameter Width = new NoInterpClampedFloatParameter(0.5f, 0.0f, 1.0f, false);
        public NoInterpClampedFloatParameter Height = new NoInterpClampedFloatParameter(0.5f, 0.0f, 1.0f, false);
        public NoInterpClampedFloatParameter EdgeRadius = new NoInterpClampedFloatParameter(0.1f, 0.0f, 1.0f, false);

        public override bool IsActive() => (Opacity.value > 0.0f);
    }

    [PostProcessEffect(typeof(Rectangle))]
    public class RectangleEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Rectangle";

        private readonly int _opacity = Shader.PropertyToID("_Opacity");
        private readonly int _aspectRatio = Shader.PropertyToID("_AspectRatio");
        private readonly int _width = Shader.PropertyToID("_Width");
        private readonly int _height = Shader.PropertyToID("_Height");
        private readonly int _edgeRadius = Shader.PropertyToID("_EdgeRadius");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out Rectangle component, out material);

            if (active) {
                material.SetFloat(_opacity, component.Opacity.value);
                material.SetFloat(_aspectRatio, component.AspectRatio.value);
                material.SetFloat(_width, component.Width.value);
                material.SetFloat(_height, component.Height.value);
                material.SetFloat(_edgeRadius, component.EdgeRadius.value);
            }

            return active;
        }
    }
}
