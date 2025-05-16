using UnityEngine;

#pragma warning disable IDE1006     // Naming Styles

namespace EasyMaterials {

    public class Log {

        private const string Prefix = "Easy Materials: ";

        public static bool DebugLogging;

        public static void d(string msg) {
            if (DebugLogging) Debug.Log(Prefix + msg);
        }

        public static void i(string msg) {
            Debug.Log(Prefix + msg);
        }

        public static void w(string msg) {
            Debug.LogWarning(Prefix + msg);
        }

        public static void e(string msg) {
            Debug.LogError(Prefix + msg);
        }
    }
}

#pragma warning restore IDE1006     // Naming Styles
