using System;
using System.Diagnostics;
using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// If you want to print logs, it needs to be Project Setting -> Scripting Define Symbols "ENABLE_LOG" defined.
    /// And You can freely customize 'ENABLE_LOG
    /// </summary>
    public static class CustomLog
    {
        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void Log(object message)
        {
            // UnityEngine.Debug.Log(message);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void Log(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.Log(message, context);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogError(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogError(message, context);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogAssertion(object message)
        {
            UnityEngine.Debug.LogAssertion(message);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogAssertion(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogAssertion(message, context);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogException(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogException(Exception exception, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogException(exception, context);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogFormat(LogType logType, LogOption logOption, UnityEngine.Object context, string format,
            params object[] args)
        {
            UnityEngine.Debug.LogFormat(logType, logOption, context, format, args);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogAssertionFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogAssertionFormat(context, format, args);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogAssertionFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogAssertionFormat(format, args);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(context, format, args);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogErrorFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(format, args);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(context, format, args);
        }

        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogWarningFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(format, args);
        }
    }
}