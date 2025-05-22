using System;
using UnityEngine.Scripting;

namespace Nexcide.PostProcessing {

    public class PostProcessEffectAttribute : PreserveAttribute {

        public readonly Type VolumeComponentType;

        public PostProcessEffectAttribute(Type volumeComponentType) {
            VolumeComponentType = volumeComponentType;
        }
    }
}
