using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Assistant
{
    /// <summary>
    /// Minimal hostfxr interop: starts the .NET runtime in-process (framework
    /// dependent) and loads the ClassicAssist entry point. Mirrors the flow from
    /// "Write a custom .NET runtime host" (learn.microsoft.com):
    ///   hostfxr_initialize_for_runtime_config -> hostfxr_get_runtime_delegate
    ///   (hdt_load_assembly_and_get_function_pointer) -> load_assembly_and_get_function_pointer
    /// </summary>
    internal static class HostFxr
    {
        private const int HDT_LOAD_ASSEMBLY_AND_GET_FUNCTION_POINTER = 5;

        // dotnet/runtime src/native/corehost/error_codes.h
        private const int SUCCESS = 0;
        private const int SUCCESS_HOST_ALREADY_INITIALIZED = 1;
        private const int SUCCESS_DIFFERENT_RUNTIME_PROPERTIES = 2;
        private const int CORE_HOST_LIB_MISSING_FAILURE = unchecked( (int) 0x80008083 );
        private const int INVALID_CONFIG_FILE = unchecked( (int) 0x80008093 );
        private const int FRAMEWORK_MISSING_FAILURE = unchecked( (int) 0x80008096 );

        private const string RUNTIME_CONFIG_FILE = "ClassicAssist.runtimeconfig.json";
        private const string ASSEMBLY_FILE = "ClassicAssist.dll";
        private const string ENTRY_TYPE_NAME = "Assistant.EngineCore, ClassicAssist";
        private const string ENTRY_METHOD_NAME = "Install";
        private const string ENTRY_DELEGATE_TYPE_NAME = "Assistant.ComponentEntryPoint, ClassicAssist";

        // hostfxr's char_t is UTF-16 (wchar_t) on Windows and UTF-8 (char) on other
        // platforms, so the string-taking hosting APIs need one delegate per encoding,
        // selected at runtime.
        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        private delegate int InitializeForRuntimeConfigUtf16(
            [MarshalAs( UnmanagedType.LPWStr )] string runtimeConfigPath,
            IntPtr parameters,
            out IntPtr hostContextHandle );

        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        private delegate int InitializeForRuntimeConfigUtf8(
            [MarshalAs( UnmanagedType.LPStr )] string runtimeConfigPath,
            IntPtr parameters,
            out IntPtr hostContextHandle );

        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        private delegate int GetRuntimeDelegate(
            IntPtr hostContextHandle,
            int delegateType,
            out IntPtr delegatePointer );

        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        private delegate int CloseHostFxr( IntPtr hostContextHandle );

        // Assembly-qualified name of the managed delegate type (in ClassicAssist) the entry point
        // is wrapped in. Passing it makes hostfxr return a native-callable thunk with a proper
        // native/managed transition — do NOT switch back to the null delegate type: that path
        // requires [UnmanagedCallersOnly] on the entry point, which CoreCLR cannot call through a
        // managed delegate and crashes or surfaces an SEHException in this bootstrapper.
        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        private delegate int LoadAssemblyAndGetFunctionPointerUtf16(
            [MarshalAs( UnmanagedType.LPWStr )] string assemblyPath,
            [MarshalAs( UnmanagedType.LPWStr )] string typeName,
            [MarshalAs( UnmanagedType.LPWStr )] string methodName,
            [MarshalAs( UnmanagedType.LPWStr )] string delegateTypeName,
            IntPtr reserved,
            out IntPtr functionPointer );

        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        private delegate int LoadAssemblyAndGetFunctionPointerUtf8(
            [MarshalAs( UnmanagedType.LPStr )] string assemblyPath,
            [MarshalAs( UnmanagedType.LPStr )] string typeName,
            [MarshalAs( UnmanagedType.LPStr )] string methodName,
            [MarshalAs( UnmanagedType.LPStr )] string delegateTypeName,
            IntPtr reserved,
            out IntPtr functionPointer );

        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        public delegate int ComponentEntryPoint( IntPtr args, int sizeBytes );

        public static bool TryInstall( string pluginDirectory, IntPtr pluginHeader, out string error )
        {
            error = null;

            string runtimeConfigPath = Path.Combine( pluginDirectory, RUNTIME_CONFIG_FILE );

            if ( !File.Exists( runtimeConfigPath ) )
            {
                error = $"{RUNTIME_CONFIG_FILE} was not found next to ClassicAssist.Proxy.dll ({pluginDirectory}).";
                return false;
            }

            string assemblyPath = Path.Combine( pluginDirectory, ASSEMBLY_FILE );

            if ( !File.Exists( assemblyPath ) )
            {
                error = $"{ASSEMBLY_FILE} was not found next to ClassicAssist.Proxy.dll ({pluginDirectory}).";
                return false;
            }

            string hostFxrPath = FindHostFxr( pluginDirectory );

            if ( hostFxrPath == null )
            {
                error = "Could not locate the hostfxr library (hostfxr.dll / libhostfxr.so / libhostfxr.dylib). Install the .NET 11 runtime (x64) or set the DOTNET_ROOT environment variable to the .NET installation directory.";
                return false;
            }

            IntPtr module = NativeLoader.LoadLibrary( hostFxrPath );

            if ( module == IntPtr.Zero )
            {
                error = $"Failed to load {hostFxrPath}.";
                return false;
            }

            // hostfxr is intentionally never unloaded — the runtime keeps state in it.

            IntPtr initializePtr = NativeLoader.GetProcAddress( module, "hostfxr_initialize_for_runtime_config" );
            IntPtr getDelegatePtr = NativeLoader.GetProcAddress( module, "hostfxr_get_runtime_delegate" );
            IntPtr closePtr = NativeLoader.GetProcAddress( module, "hostfxr_close" );

            if ( initializePtr == IntPtr.Zero || getDelegatePtr == IntPtr.Zero || closePtr == IntPtr.Zero )
            {
                error = $"hostfxr at {hostFxrPath} does not export the required hosting API.";
                return false;
            }

            GetRuntimeDelegate getDelegate = (GetRuntimeDelegate) Marshal.GetDelegateForFunctionPointer( getDelegatePtr, typeof( GetRuntimeDelegate ) );
            CloseHostFxr close = (CloseHostFxr) Marshal.GetDelegateForFunctionPointer( closePtr, typeof( CloseHostFxr ) );

            int rc = InitializeRuntime( initializePtr, runtimeConfigPath, out IntPtr context );

            if ( rc != SUCCESS && rc != SUCCESS_HOST_ALREADY_INITIALIZED && rc != SUCCESS_DIFFERENT_RUNTIME_PROPERTIES )
            {
                error = DescribeError( rc, runtimeConfigPath );
                return false;
            }

            try
            {
                rc = getDelegate( context, HDT_LOAD_ASSEMBLY_AND_GET_FUNCTION_POINTER, out IntPtr loadAssemblyPtr );

                if ( rc != SUCCESS )
                {
                    error = DescribeError( rc, runtimeConfigPath );
                    return false;
                }

                rc = LoadAssemblyAndGetEntryPoint( loadAssemblyPtr, assemblyPath, out IntPtr entryPtr );

                if ( rc != SUCCESS )
                {
                    error = DescribeError( rc, assemblyPath );
                    return false;
                }

                ComponentEntryPoint entry = (ComponentEntryPoint) Marshal.GetDelegateForFunctionPointer( entryPtr, typeof( ComponentEntryPoint ) );

                int result = entry( pluginHeader, 0 );

                if ( result != 0 )
                {
                    error = $"ClassicAssist initialization failed (exit code {result}). See the ClassicUO console output for details.";
                    return false;
                }

                return true;
            }
            finally
            {
                // Closing the context does not unload the runtime; it only releases the host context.
                close( context );
            }
        }

        private static int InitializeRuntime( IntPtr initializePtr, string runtimeConfigPath, out IntPtr context )
        {
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                InitializeForRuntimeConfigUtf16 initialize = (InitializeForRuntimeConfigUtf16) Marshal.GetDelegateForFunctionPointer( initializePtr, typeof( InitializeForRuntimeConfigUtf16 ) );
                return initialize( runtimeConfigPath, IntPtr.Zero, out context );
            }

            InitializeForRuntimeConfigUtf8 initializeUtf8 = (InitializeForRuntimeConfigUtf8) Marshal.GetDelegateForFunctionPointer( initializePtr, typeof( InitializeForRuntimeConfigUtf8 ) );
            return initializeUtf8( runtimeConfigPath, IntPtr.Zero, out context );
        }

        private static int LoadAssemblyAndGetEntryPoint( IntPtr loadAssemblyPtr, string assemblyPath, out IntPtr entryPtr )
        {
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                LoadAssemblyAndGetFunctionPointerUtf16 loadAssembly = (LoadAssemblyAndGetFunctionPointerUtf16) Marshal.GetDelegateForFunctionPointer( loadAssemblyPtr, typeof( LoadAssemblyAndGetFunctionPointerUtf16 ) );
                return loadAssembly( assemblyPath, ENTRY_TYPE_NAME, ENTRY_METHOD_NAME, ENTRY_DELEGATE_TYPE_NAME, IntPtr.Zero, out entryPtr );
            }

            LoadAssemblyAndGetFunctionPointerUtf8 loadAssemblyUtf8 = (LoadAssemblyAndGetFunctionPointerUtf8) Marshal.GetDelegateForFunctionPointer( loadAssemblyPtr, typeof( LoadAssemblyAndGetFunctionPointerUtf8 ) );
            return loadAssemblyUtf8( assemblyPath, ENTRY_TYPE_NAME, ENTRY_METHOD_NAME, ENTRY_DELEGATE_TYPE_NAME, IntPtr.Zero, out entryPtr );
        }

        private static string FindHostFxr( string pluginDirectory )
        {
            string hostFxrFileName = GetHostFxrFileName();

            // App-local layout (self-contained style deployments).
            string appLocal = Path.Combine( pluginDirectory, hostFxrFileName );

            if ( File.Exists( appLocal ) )
            {
                return appLocal;
            }

            foreach ( string root in GetDotNetRoots() )
            {
                if ( string.IsNullOrEmpty( root ) )
                {
                    continue;
                }

                string hostFxr = FindHighestVersionHostFxr( Path.Combine( root, "host", "fxr" ), hostFxrFileName );

                if ( hostFxr != null )
                {
                    return hostFxr;
                }
            }

            return null;
        }

        private static string GetHostFxrFileName()
        {
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                return "hostfxr.dll";
            }

            if ( RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) )
            {
                return "libhostfxr.dylib";
            }

            return "libhostfxr.so";
        }

        private static string[] GetDotNetRoots()
        {
            string archRoot = Environment.GetEnvironmentVariable( "DOTNET_ROOT_X64" );
            string genericRoot = Environment.GetEnvironmentVariable( "DOTNET_ROOT" );

            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                return
                [
                    archRoot,
                    genericRoot,
                    Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ProgramFiles ), "dotnet" )
                ];
            }

            if ( RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) )
            {
                return [archRoot, genericRoot, "/usr/local/share/dotnet"];
            }

            // Linux: system package installs and user/script installs.
            return [archRoot, genericRoot, "/usr/share/dotnet", "/usr/lib/dotnet"];
        }

        private static string FindHighestVersionHostFxr( string fxrDirectory, string hostFxrFileName )
        {
            if ( !Directory.Exists( fxrDirectory ) )
            {
                return null;
            }

            string bestDirectory = null;
            Version bestVersion = null;

            foreach ( string directory in Directory.GetDirectories( fxrDirectory ) )
            {
                if ( !File.Exists( Path.Combine( directory, hostFxrFileName ) ) )
                {
                    continue;
                }

                if ( !Version.TryParse( Path.GetFileName( directory ), out Version version ) )
                {
                    continue;
                }

                if ( bestVersion == null || version > bestVersion )
                {
                    bestVersion = version;
                    bestDirectory = directory;
                }
            }

            return bestDirectory == null ? null : Path.Combine( bestDirectory, hostFxrFileName );
        }

        /// <summary>
        /// Cross-platform LoadLibrary/GetProcAddress, mirroring ClassicUO.Bootstrap's
        /// LibraryLoader: kernel32 on Windows, libdl (dlopen/dlsym) everywhere else.
        /// </summary>
        private static class NativeLoader
        {
            private const int RTLD_NOW = 0x002;
            private const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;

            private delegate IntPtr LoadLibraryDelegate( string fileName );

            private delegate IntPtr GetProcAddressDelegate( IntPtr module, string name );

            private static readonly LoadLibraryDelegate _loadLibrary;
            private static readonly GetProcAddressDelegate _getProcAddress;

            static NativeLoader()
            {
                if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
                {
                    _loadLibrary = LoadLibraryWindows;
                    _getProcAddress = GetProcAddressWindows;
                    return;
                }

                // Probe libdl first, then fall back to libdl.so.2 (same as the bootstrapper).
                try
                {
                    LibDl.dlerror();
                    _loadLibrary = fileName => LibDl.dlopen( fileName, RTLD_NOW );
                    _getProcAddress = LibDl.dlsym;
                }
                catch
                {
                    _loadLibrary = fileName => LibDl2.dlopen( fileName, RTLD_NOW );
                    _getProcAddress = LibDl2.dlsym;
                }
            }

            public static IntPtr LoadLibrary( string fileName )
            {
                return _loadLibrary( fileName );
            }

            public static IntPtr GetProcAddress( IntPtr module, string name )
            {
                return _getProcAddress( module, name );
            }

            private static IntPtr LoadLibraryWindows( string fileName )
            {
                return LoadLibraryExW( fileName, IntPtr.Zero, LOAD_WITH_ALTERED_SEARCH_PATH );
            }

            private static IntPtr GetProcAddressWindows( IntPtr module, string name )
            {
                return GetProcAddressWin32( module, name );
            }

            [DllImport( "kernel32", SetLastError = true, CharSet = CharSet.Unicode )]
            [DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
            private static extern IntPtr LoadLibraryExW( string fileName, IntPtr hFile, uint flags );

            [DllImport( "kernel32", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi, SetLastError = true )]
            private static extern IntPtr GetProcAddressWin32( IntPtr module, string name );

            private const string LIB_DL = "libdl";
            private const string LIB_DL_2 = "libdl.so.2";

            private static class LibDl
            {
                [DllImport( LIB_DL )]
                public static extern IntPtr dlopen( string fileName, int flags );

                [DllImport( LIB_DL )]
                public static extern IntPtr dlsym( IntPtr handle, string name );

                [DllImport( LIB_DL )]
                public static extern int dlerror();
            }

            private static class LibDl2
            {
                [DllImport( LIB_DL_2 )]
                public static extern IntPtr dlopen( string fileName, int flags );

                [DllImport( LIB_DL_2 )]
                public static extern IntPtr dlsym( IntPtr handle, string name );

                [DllImport( LIB_DL_2 )]
                public static extern int dlerror();
            }
        }

        private static string DescribeError( int resultCode, string path )
        {
            string reason;

            switch ( resultCode )
            {
                case FRAMEWORK_MISSING_FAILURE:
                    reason = "The required .NET runtime was not found. Install the .NET 11 runtime (x64).";
                    break;

                case INVALID_CONFIG_FILE:
                    reason = "The runtime configuration file is invalid.";
                    break;

                case CORE_HOST_LIB_MISSING_FAILURE:
                    reason = "A .NET hosting component is missing or the .NET installation is incomplete.";
                    break;

                default:
                    reason = "Unknown hosting error.";
                    break;
            }

            return $"Failed to start the .NET runtime for ClassicAssist (0x{resultCode:X8}). {reason}{Environment.NewLine}{path}";
        }
    }
}
