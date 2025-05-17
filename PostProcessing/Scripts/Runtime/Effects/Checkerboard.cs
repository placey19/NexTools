using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [VolumeComponentMenu("Nexcide/Checkerboard")]
    public class Checkerboard : VolumeComponentBase {

        public ClampedFloatParameter Blend = new(0.0f, 0.0f, 1.0f);
        public NoInterpFloatParameter Size = new(25.0f);
        public ColorParameter Color1 = new(Color.black);
        public ColorParameter Color2 = new(Color.white);
        public NoInterpVector2Parameter Offset = new(Vector2.zero);

        public override bool IsActive() => (Blend.value > 0.0f);
    }

    [PostProcessEffect(typeof(Checkerboard))]
    public class CheckerboardEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Checkerboard";

        private static readonly int _blend = Shader.PropertyToID("_Blend");
        private static readonly int _size = Shader.PropertyToID("_Size");
        private static readonly int _color1 = Shader.PropertyToID("_Color1");
        private static readonly int _color2 = Shader.PropertyToID("_Color2");
        private static readonly int _offset = Shader.PropertyToID("_Offset");

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
