using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using CUO_API;

namespace Assistant
{
    /// <summary>
    /// Proxy Handler from net4 to netcore and back. 
    /// 
    /// Loaded by the ClassicUO .NET Framework bootstrapper via reflection:
    /// Assembly.LoadFile -> GetType("Assistant.Engine") -> GetMethod("Install") -> Invoke.
    /// Its only job is to start the .NET runtime in-process and hand the PluginHeader
    /// pointer over to ClassicAssist (netcore) — see Assistant.EngineCore.Install.
    /// </summary>
    /// <remarks>
    /// This can be removed as soon as ClassicUO is able to load a dotnetcore Assistant.Engine.
    /// </remarks>
    public static unsafe class Engine
    {
        private static bool _installAttempted;

        public static void Install( PluginHeader* plugin )
        {
            if ( _installAttempted )
            {
                return;
            }

            _installAttempted = true;

            string pluginDirectory = Path.GetDirectoryName( typeof( Engine ).Assembly.Location );

            if ( string.IsNullOrEmpty( pluginDirectory ) )
            {
                ShowError( "Could not determine the ClassicAssist plugin directory." );
                return;
            }

            if ( !HostFxr.TryInstall( pluginDirectory, (IntPtr) plugin, out string error ) )
            {
                ShowError( error );
            }
        }

        private static void ShowError( string message )
        {
            try
            {
                Console.WriteLine( $"ERROR: {message}" );
            }
            catch
            {
                // No console attached — ignore.
            }

            ShowErrorMessageBox( message );
        }

        [Conditional("DEBUG")]
        private static void ShowErrorMessageBox( string message )
        {
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                const uint MB_ICONERROR = 0x10;
                // Windows only — the bootstrapper runs without a console on normal launches,
                // so make the failure visible with a message box. On other platforms the
                // ClassicUO console is the error surface.
                MessageBox( IntPtr.Zero, message, "ClassicAssist", MB_ICONERROR );
            }
        }

        [DllImport( "user32", CharSet = CharSet.Unicode )]
        private static extern int MessageBox( IntPtr hWnd, string text, string caption, uint type );
    }
}
