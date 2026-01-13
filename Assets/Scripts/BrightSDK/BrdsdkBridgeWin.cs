using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Brdsdk
{
    public class BrdsdkBridgeWin
    {
        public enum Choice : int
        {
            None = 0,
            Peer = 1,
            NotPeer = 2,
        }
        public enum ServiceStatus : int
        {
            None = 0,
            NotInstalled = 1,
            Installed = 2,
            NotRunning = 3,
            Running = 4,
            Disconnected = 5,
            Blocked = 6,
            Connected = 7,
            Peer = 8,
        }

        public delegate void ChoiceChangeCallback(BrdsdkBridgeWin.Choice choice);
        public delegate void ServiceStatusChangeCallback(BrdsdkBridgeWin.ServiceStatus status);

        public static Choice choice {
            get {
#if UNITY_STANDALONE_WIN
                if (Environment.Is64BitProcess)
                {
                    return (Choice)_NativeImportsWin64.brd_sdk_get_consent_choice_c();
                }
                else
                {
                    return (Choice)_NativeImportsWin32.brd_sdk_get_consent_choice_c();
                }
#else
                return Choice.None;
#endif
            }
        }

        public static void Init(bool skipConsent)
        {
#if UNITY_STANDALONE_WIN
            if (Environment.Is64BitProcess)
            {
                _NativeImportsWin64.brd_sdk_set_skip_consent_on_init_c(skipConsent);
                _NativeImportsWin64.brd_sdk_set_choice_change_cb_c(sdkChoiceCallback);
                _NativeImportsWin64.brd_sdk_set_service_status_change_cb_c(sdkStatusChangeCallback);
                _NativeImportsWin64.brd_sdk_init_c();
            }
            else
            {
                _NativeImportsWin32.brd_sdk_set_skip_consent_on_init_c(skipConsent);    
                _NativeImportsWin32.brd_sdk_set_choice_change_cb_c(sdkChoiceCallback);
                _NativeImportsWin32.brd_sdk_set_service_status_change_cb_c(sdkStatusChangeCallback);
                _NativeImportsWin32.brd_sdk_init_c();
            }
#endif
        }

        public static void ShowConsent() 
        {
#if UNITY_STANDALONE_WIN
            if (Environment.Is64BitProcess)
            {
                _NativeImportsWin64.brd_sdk_show_consent_c();
            }
            else
            {
                _NativeImportsWin32.brd_sdk_show_consent_c();
            }
#endif
        }

        public static void OptOut()
        {
#if UNITY_STANDALONE_WIN
            if (Environment.Is64BitProcess)
            {
                _NativeImportsWin64.brd_sdk_opt_out_c();
            }
            else
            {
                _NativeImportsWin32.brd_sdk_opt_out_c();
            }
#endif
        }

        public static void FixService()
        {
#if UNITY_STANDALONE_WIN
            if (Environment.Is64BitProcess)
            {
                _NativeImportsWin64.brd_sdk_fix_service_status_c();
            }
            else
            {
                _NativeImportsWin32.brd_sdk_fix_service_status_c();
            }
#endif
        }

        public static void Deinit()
        {
#if UNITY_STANDALONE_WIN
            if (Environment.Is64BitProcess)
            {
                _NativeImportsWin64.brd_sdk_close_c();
            }
            else
            {
                _NativeImportsWin32.brd_sdk_close_c();
            }
#endif
        }

        public static void SetChoiceChangeCallback(BrdsdkBridgeWin.ChoiceChangeCallback callback)
        {
            onChoiceChange = callback;
        }
        private static BrdsdkBridgeWin.ChoiceChangeCallback onChoiceChange;
        [MonoPInvokeCallback(typeof(BrdsdkBridgeWin.ChoiceChangeCallback))]
        private static void sdkChoiceCallback(BrdsdkBridgeWin.Choice choice)
        {
            if (onChoiceChange != null)
                onChoiceChange(choice);
        }

        public static void SetStatusChangeCallback(BrdsdkBridgeWin.ServiceStatusChangeCallback callback)
        {
            onStatusChange = callback;
        }
        private static BrdsdkBridgeWin.ServiceStatusChangeCallback onStatusChange;
        [MonoPInvokeCallback(typeof(BrdsdkBridgeWin.ServiceStatusChangeCallback))]
        private static void sdkStatusChangeCallback(BrdsdkBridgeWin.ServiceStatus status)
        {
            if (onStatusChange != null)
                onStatusChange(status);
        }
    }

#if UNITY_STANDALONE_WIN
    static class _NativeImportsWin32
    {
        private const string lumDllName = "lum_sdk32";
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void brd_sdk_set_skip_consent_on_init_c(bool skip);
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void brd_sdk_init_c();
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void brd_sdk_show_consent_c();
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int brd_sdk_get_consent_choice_c();
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int brd_sdk_opt_out_c();
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int brd_sdk_close_c();
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int brd_sdk_fix_service_status_c();
        
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void brd_sdk_set_choice_change_cb_c(BrdsdkBridgeWin.ChoiceChangeCallback callback);
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void brd_sdk_set_service_status_change_cb_c(BrdsdkBridgeWin.ServiceStatusChangeCallback callback);
    }
    static class _NativeImportsWin64
    {
        private const string lumDllName = "lum_sdk64";
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void brd_sdk_set_skip_consent_on_init_c(bool skip);
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void brd_sdk_init_c();
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void brd_sdk_show_consent_c();
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int brd_sdk_get_consent_choice_c();
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int brd_sdk_opt_out_c();
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int brd_sdk_close_c();
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int brd_sdk_fix_service_status_c();
        
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void brd_sdk_set_choice_change_cb_c(BrdsdkBridgeWin.ChoiceChangeCallback callback);
        [DllImport(lumDllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void brd_sdk_set_service_status_change_cb_c(BrdsdkBridgeWin.ServiceStatusChangeCallback callback);
    }
#endif
}