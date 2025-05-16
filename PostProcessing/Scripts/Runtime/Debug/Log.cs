using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

#pragma warning disable IDE1006     // Naming Styles

namespace Nexcide.PostProcessing {

    public enum LogLevel {

        /// <summary>
        /// Only errors, warnings & info logging will be shown.
        /// </summary>
        Default,

        /// <summary>
        /// Debug level logging Log.d() included.
        /// </summary>
        Debug,

        /// <summary>
        /// All logging including Log.v() statements.
        /// </summary>
        Verbose
    }

    public class Log {

        private static string Name(ScriptableObject so) {
            return (!string.IsNullOrEmpty(so.name) ? so.name : nameof(ScriptableObject));
        }

        // raw logging functions
        public static void e(string msg) {
            Debug.LogError(msg);
        }

        public static void w(string msg) {
            Debug.LogWarning(msg);
        }

        public static void i(string msg) {
            Debug.Log(msg);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void d(string msg) {
            Debug.Log(msg);
        }

        // ScriptableObject logging functions
        public static void e(LogLevel logLevel, ScriptableObject so, string msg) {
            if (logLevel >= LogLevel.Default) {
                Debug.LogError($"E/{{{Name(so)}}} {msg}");
            }
        }

        public static void w(LogLevel logLevel, ScriptableObject so, string msg) {
            if (logLevel >= LogLevel.Default) {
                Debug.LogWarning($"W/{{{Name(so)}}} {msg}");
            }
        }

        public static void i(LogLevel logLevel, ScriptableObject so, string msg) {
            if (logLevel >= LogLevel.Default) {
                Debug.Log($"I/{{{Name(so)}}} {msg}");
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void d(LogLevel logLevel, ScriptableObject so, string msg) {
            if (logLevel >= LogLevel.Debug) {
                Debug.Log($"D/{{{Name(so)}}} {msg}");
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void v(LogLevel logLevel, ScriptableObject so, string msg) {
            if (logLevel >= LogLevel.Verbose) {
                Debug.Log($"V/{{{Name(so)}}} {msg}");
            }
        }

        // raw logging for ScriptableObject-derived components (includes Editors & EditorWindows)
        public static void e(ScriptableObject so, string msg) {
            Debug.LogError($"{{{Name(so)}}} {msg}");
        }

        public static void w(ScriptableObject so, string msg) {
            Debug.LogWarning($"{{{Name(so)}}} {msg}");
        }

        public static void d(ScriptableObject so, string msg) {
            Debug.Log($"D/{{{Name(so)}}} {msg}");
        }
    }
}

#pragma warning restore IDE1006     // Naming Styles
