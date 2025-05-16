using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Nexcide.PostProcessing {

    [CreateAssetMenu(menuName = "Nexcide/Shader List")]
    public class ShaderList : ScriptableObject {

        public List<Shader> Shaders = new();

        public List<VolumeEffect> CreateVolumeEffects() {
            List<VolumeEffect> volumeEffects = new();

            if (Shaders != null && Shaders.Count > 0) {
                volumeEffects = new();

                // need to find and create all VolumeEffects and filter by checking to see if the shader names are a match
                List<VolumeEffect> allVolumeEffects = CreateAllVolumeEffects();

                foreach (Shader shader in Shaders) {
                    if (IsShaderInList(allVolumeEffects, shader, out VolumeEffect effect)) {
                        volumeEffects.Add(effect);
                    } else {
                        Log.w($"Failed to find {nameof(VolumeEffect)} for shader: {shader.name}");
                    }
                }
            }

            return volumeEffects;
        }

        public static List<VolumeEffect> CreateAllVolumeEffects() {
            List<VolumeEffect> volumeEffects = new();
            List<Type> effectTypes = FindAllEffectTypes();

            foreach (Type effectType in effectTypes) {
                if (ConstructVolumeEffect(effectType, out VolumeEffect effect)) {
                    volumeEffects.Add(effect);
                } else {
                    Log.e($"Failed to construct {nameof(VolumeEffect)} of type: {effectType}");
                }
            }

            return volumeEffects;
        }

        public static bool ConstructVolumeEffect(Type effectType, out VolumeEffect effect) {
            effect = null;

            try {
                effect = (VolumeEffect)Activator.CreateInstance(effectType);
            } catch (Exception e) {
                Log.e(e.ToString());
            }

            return (effect != null);
        }

        private static bool IsShaderInList(List<VolumeEffect> effects, Shader shader, out VolumeEffect foundEffect) {
            foundEffect = null;

            foreach (VolumeEffect effect in effects) {
                if (effect.ShaderName == shader.name) {
                    foundEffect = effect;
                    break;
                }
            }

            return (foundEffect != null);
        }

        private static List<Type> FindAllEffectTypes() {
            List<Type> allEffectTypes = new();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies) {
                IEnumerable<Type> effectTypes = assembly.GetTypes().Where(t => t.IsDefined(typeof(PostProcessEffectAttribute)));
                allEffectTypes.AddRange(effectTypes);
            }

            return allEffectTypes.ToList();
        }
    }
}
