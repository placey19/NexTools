using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [Serializable, VolumeComponentMenu("Nexcide/Checkerboard")]
    public class Checkerboard : VolumeComponentBase {

        public FloatParameter Blend = new ClampedFloatParameter(0.0f, 0.0f, 1.0f, false);
        public NoInterpFloatParameter Size = new NoInterpFloatParameter(25.0f, false);
        public ColorParameter Color1 = new ColorParameter(Color.black, false);
        public ColorParameter Color2 = new ColorParameter(Color.white, false);
        public NoInterpVector2Parameter Offset = new NoInterpVector2Parameter(Vector2.zero, false);

        public override bool IsActive() => (Blend.value > 0.0f);
    }

    [PostProcessEffect(typeof(Checkerboard))]
    public class CheckerboardEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Checkerboard";

        private readonly int _blend = Shader.PropertyToID("_Blend");
        private readonly int _size = Shader.PropertyToID("_Size");
        private readonly int _color1 = Shader.PropertyToID("_Color1");
        private readonly int _color2 = Shader.PropertyToID("_Color2");
        private readonly int _offset = Shader.PropertyToID("_Offset");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out Checkerboard component, out material);

            if (active) {
                material.SetFloat(_blend, component.Blend.value);
                material.SetFloat(_size, component.Size.value);
                material.SetColor(_color1, component.Color1.value);
                material.SetColor(_color2, component.Color2.value);
                material.SetVector(_offset, component.Offset.value);
            }

            return active;
        }
    }
}
