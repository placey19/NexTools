using System;

using static Nexcide.NexTools.ReflectionUtil;

namespace Nexcide.NexTools {

    public static class ProGridsUtil {

        private static readonly string[] ProGridsEditorTypeNames = new string[] {

            "UnityEditor.ProGrids.ProGridsEditor",
            "ProGrids.Editor.ProGridsEditor",
            "ProGrids.Editor.pg_Editor",
            "ProGrids.pg_Editor",
            "pg_Editor",
        };

        private static Type _proGridsEditor;
        private static Func<bool> _proGridsActive;
        private static Func<bool> _proGridsSnapEnabled;
        private static Func<float> _proGridsSnapValue;

        static ProGridsUtil() {
            for (int i = 0; i < ProGridsEditorTypeNames.Length; ++i) {
                _proGridsEditor = GetTypeFromAssembly(ProGridsEditorTypeNames[i]);

                if (_proGridsEditor != null) {
                    break;
                }
            }
        }

        public static bool IsActive() {
            bool isActive = false;

            if (_proGridsEditor != null) {
                _proGridsActive ??= _proGridsEditor.CreateDelegate<Func<bool>>("SceneToolbarActive");

                if (_proGridsActive != null) {
                    return _proGridsActive();
                }
            }

            return isActive;
        }

        public static bool SnapEnabled() {
            bool snapEnabled = false;

            if (IsActive()) {
                _proGridsSnapEnabled ??= _proGridsEditor.CreateDelegate<Func<bool>>("SnapEnabled");

                if (_proGridsSnapEnabled != null) {
                    snapEnabled = _proGridsSnapEnabled();
                }
            }

            return snapEnabled;
        }

        public static bool SnapValue(out float snapValue) {
            bool success = false;
            snapValue = 0.0f;

            if (SnapEnabled()) {
                _proGridsSnapValue ??= _proGridsEditor.CreateDelegate<Func<float>>("SnapValue");

                if (_proGridsSnapValue != null) {
                    snapValue = _proGridsSnapValue();
                    success = true;
                }
            }

            return success;
        }
    }
}
