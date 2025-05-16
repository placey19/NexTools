using System.Diagnostics;
using UnityEngine;

namespace Nexcide {

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

    /// <summary>
    /// Change Unity logging into Java-Android style logging
    /// </summary>
    public class Log {

        private static string Name(ScriptableObject so) {
            return (!string.IsNullOrEmpty(so.name) ? so.name : nameof(ScriptableObject));
        }

        // raw logging functions
        public static void e(string msg) {
            UnityEngine.Debug.LogError(msg);
        }

        public static void w(string msg) {
            UnityEngine.Debug.LogWarning(msg);
        }

        public static void i(string msg) {
            UnityEngine.Debug.Log(msg);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void d(string msg) {
            UnityEngine.Debug.Log(msg);
        }

        // raw logging functions with tag
        public static void e(string tag, string msg) {
            UnityEngine.Debug.LogError($"[{tag}] {msg}");
        }

        public static void w(string tag, string msg) {
            UnityEngine.Debug.LogWarning($"[{tag}] {msg}");
        }

        public static void i(string tag, string msg) {
            UnityEngine.Debug.Log($"[{tag}] {msg}");
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void d(string tag, string msg) {
            UnityEngine.Debug.Log($"[{tag}] {msg}");
        }

        // raw logging for ScriptableObject-derived components (includes Editors & EditorWindows)
        public static void e(ScriptableObject so, string msg) {
            UnityEngine.Debug.LogError($"{{{Name(so)}}} {msg}");
        }

        public static void w(ScriptableObject so, string msg) {
            UnityEngine.Debug.LogWarning($"{{{Name(so)}}} {msg}");
        }

        public static void d(ScriptableObject so, string msg) {
            UnityEngine.Debug.Log($"D/{{{Name(so)}}} {msg}");
        }

        // MonoBehaviour logging functions with LogLevel
        public static void e(LogLevel logLevel, Component component, string msg) {
            if (logLevel >= LogLevel.Default) {
                UnityEngine.Debug.LogError($"E/[{component.gameObject.name}] {component.GetType().Name}: {msg}");
            }
        }

        public static void w(LogLevel logLevel, Component component, string msg) {
            if (logLevel >= LogLevel.Default) {
                UnityEngine.Debug.LogWarning($"W/[{component.gameObject.name}] {component.GetType().Name}: {msg}");
            }
        }

        public static void i(LogLevel logLevel, Component component, string msg) {
            if (logLevel >= LogLevel.Default) {
                UnityEngine.Debug.Log($"I/[{component.gameObject.name}] {component.GetType().Name}: {msg}");
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void d(LogLevel logLevel, Component component, string msg) {
            if (logLevel >= LogLevel.Debug) {
                UnityEngine.Debug.Log($"D/[{component.gameObject.name}] {component.GetType().Name}: {msg}");
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void v(LogLevel logLevel, Component component, string msg) {
            if (logLevel >= LogLevel.Verbose) {
                UnityEngine.Debug.Log($"V/[{component.gameObject.name}] {component.GetType().Name}: {msg}");
            }
        }

        // MonoBehaviour logging functions with LogLevel and tag
        public static void e(LogLevel logLevel, Component component, string tag, string msg) {
            if (logLevel >= LogLevel.Default) {
                UnityEngine.Debug.LogError($"E/[{component.gameObject.name}] {component.GetType().Name} <{tag}>: {msg}");
            }
        }

        public static void w(LogLevel logLevel, Component component, string tag, string msg) {
            if (logLevel >= LogLevel.Default) {
                UnityEngine.Debug.LogWarning($"W/[{component.gameObject.name}] {component.GetType().Name} <{tag}>: {msg}");
            }
        }

        public static void i(LogLevel logLevel, Component component, string tag, string msg) {
            if (logLevel >= LogLevel.Default) {
                UnityEngine.Debug.Log($"I/[{component.gameObject.name}] {component.GetType().Name} <{tag}>: {msg}");
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void d(LogLevel logLevel, Component component, string tag, string msg) {
            if (logLevel >= LogLevel.Debug) {
                UnityEngine.Debug.Log($"D/[{component.gameObject.name}] {component.GetType().Name} <{tag}>: {msg}");
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void v(LogLevel logLevel, Component component, string tag, string msg) {
            if (logLevel >= LogLevel.Verbose) {
                UnityEngine.Debug.Log($"V/[{component.gameObject.name}] {component.GetType().Name} <{tag}>: {msg}");
            }
        }

        // ScriptableObject logging functions
        public static void e(LogLevel logLevel, ScriptableObject so, string msg) {
            if (logLevel >= LogLevel.Default) {
                UnityEngine.Debug.LogError($"E/{{{Name(so)}}} {msg}");
            }
        }

        public static void w(LogLevel logLevel, ScriptableObject so, string msg) {
            if (logLevel >= LogLevel.Default) {
                UnityEngine.Debug.LogWarning($"W/{{{Name(so)}}} {msg}");
            }
        }

        public static void i(LogLevel logLevel, ScriptableObject so, string msg) {
            if (logLevel >= LogLevel.Default) {
                UnityEngine.Debug.Log($"I/{{{Name(so)}}} {msg}");
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void d(LogLevel logLevel, ScriptableObject so, string msg) {
            if (logLevel >= LogLevel.Debug) {
                UnityEngine.Debug.Log($"D/{{{Name(so)}}} {msg}");
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void v(LogLevel logLevel, ScriptableObject so, string msg) {
            if (logLevel >= LogLevel.Verbose) {
                UnityEngine.Debug.Log($"V/{{{Name(so)}}} {msg}");
            }
        }
    }
}
