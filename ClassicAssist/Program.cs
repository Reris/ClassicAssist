#if DEBUG
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Assistant;
using CUO_API;

namespace ClassicAssist;

/// <summary>
///     Standalone startup host for debugging.
///     <para />
///     Configuration, --name:value command line argument:
///     --classicuo — ClassicUO executable or its directory,
///     --uopath — Ultima Online client data directory,
///     --clientversion — emulated client version.
/// </summary>
/// <remarks>
///     Add
///     --classicuo:C:\Games\ClassicUO.exe --uopath:C:\Games\UltimaOnline --clientversion:7.0.104.0
///     to your launch profile.
/// </remarks>
public static class Program
{
    private static readonly Version DEFAULT_CLIENT_VERSION = new( 7, 0, 104, 0 );

    private static OnGetUOFilePath _getUOFilePath;
    private static OnGetPacketLength _getPacketLength;
    private static OnPacketSendRecv _sendToClient;
    private static OnPacketSendRecv _sendToServer;
    private static RequestMove _requestMove;
    private static OnSetTitle _setTitle;

    public static unsafe void Main( string[] args )
    {
        string classicUOAssemblyPath = GetCommandLineValue( "classicuo", args );
        string uoPath = GetCommandLineValue( "uopath", args );
        Version clientVersion = ResolveClientVersion( args );

        if ( string.IsNullOrWhiteSpace( classicUOAssemblyPath ) || !File.Exists( classicUOAssemblyPath ) )
        {
            throw new InvalidOperationException( "The ClassicUO Assembly path is not configured or not found. Set the --classicuo:<path>." );
        }

        if ( string.IsNullOrWhiteSpace( uoPath ) || !Directory.Exists( uoPath ) )
        {
            throw new InvalidOperationException( "The Ultima Online client directory is not configured or not found. Set the --uopath:<path>." );
        }

        // ReSharper disable LocalizableElement
        Console.WriteLine( "ClassicAssist standalone host" );
        Console.WriteLine( $"  ClassicUO assembly: {classicUOAssemblyPath}" );
        Console.WriteLine( $"  UO client path: {uoPath}" );
        Console.WriteLine( $"  Client version: {clientVersion}" );
        // ReSharper restore LocalizableElement

        Assembly.LoadFrom( classicUOAssemblyPath );

        PluginHeader pluginHeader = InitializePluginHeader( uoPath, clientVersion );
        Engine.Install( &pluginHeader );

        Engine.ShutdownTask.Wait();
    }

    internal static PluginHeader InitializePluginHeader( string uoPath, Version clientVersion )
    {
        _getUOFilePath = () => uoPath;
        _getPacketLength = _ => 0;
        _sendToClient = ( ref _, ref _ ) => false;
        _sendToServer = ( ref _, ref _ ) => false;
        _requestMove = ( _, _ ) => false;
        _setTitle = _ => { };

        return new PluginHeader
        {
            HWND = IntPtr.Zero,
            ClientVersion = PackClientVersion( clientVersion ),
            GetUOFilePath = Marshal.GetFunctionPointerForDelegate( _getUOFilePath ),
            GetPacketLength = Marshal.GetFunctionPointerForDelegate( _getPacketLength ),
            Recv = Marshal.GetFunctionPointerForDelegate( _sendToClient ),
            Send = Marshal.GetFunctionPointerForDelegate( _sendToServer ),
            RequestMove = Marshal.GetFunctionPointerForDelegate( _requestMove ),
            SetTitle = Marshal.GetFunctionPointerForDelegate( _setTitle )
        };
    }

    private static Version ResolveClientVersion( string[] arguments )
    {
        string value = GetCommandLineValue( "clientversion", arguments );

        if ( string.IsNullOrWhiteSpace( value ) )
        {
            return DEFAULT_CLIENT_VERSION;
        }

        if ( !Version.TryParse( value, out Version version ) )
        {
            throw new FormatException( $"Invalid client version '{value}'. Set a version like 7.0.104.0 with the --clientversion:<version> command line argument." );
        }

        return version;
    }

    private static int PackClientVersion( Version version )
    {
        int build = version.Build >= 0 ? version.Build : 0;
        int revision = version.Revision >= 0 ? version.Revision : 0;

        return ( version.Major << 24 ) | ( version.Minor << 16 ) | ( build << 8 ) | revision;
    }

    private static string GetCommandLineValue( string optionName, string[] arguments )
    {
        string prefix = $"--{optionName}";

        foreach ( string argument in arguments )
        {
            if ( !argument.StartsWith( prefix, StringComparison.OrdinalIgnoreCase ) )
            {
                continue;
            }

            string remainder = argument[prefix.Length..];

            if ( remainder.Length > 1 && ( remainder[0] == ':' || remainder[0] == '=' ) )
            {
                return remainder[1..].Trim( '"' );
            }
        }

        return null;
    }
}
#endif