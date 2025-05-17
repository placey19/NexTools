using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexcide.PostProcessing {

    [VolumeComponentMenu("Nexcide/Color Filter")]
    public class ColorFilter : VolumeComponentBase {

        public ColorParameter Color = new ColorParameter(UnityEngine.Color.white, hdr: true, showAlpha: false, showEyeDropper: false, false);

        public override bool IsActive() => (Color.value != UnityEngine.Color.white);
    }

    [PostProcessEffect(typeof(ColorFilter))]
    public class ColorFilterEffect : VolumeEffect {

        public override string ShaderName => "Nexcide/Color Filter";

        private readonly int _color = Shader.PropertyToID("_Color");

        public override bool ConfigureMaterial(VolumeStack volumeStack, out Material material) {
            bool active = ComponentActive(volumeStack, out ColorFilter component, out material);

            if (active) {
                material.SetColor(_color, component.Color.value);
            }

            return active;
        }
    }
}
