using CUO_API;
using Sentry;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

#pragma warning disable IDE0130 // Entrypoint requires a well-known name
namespace Assistant;
#pragma warning restore IDE0130

/// <summary>
/// Delegate type ClassicAssist.Proxy hands to hostfxr's load_assembly_and_get_function_pointer.
/// hostfxr wraps <see cref="EngineCore.Install"/> in a native-callable thunk for this type, which
/// gives the bootstrapper a proper native/managed transition. The entry point itself must stay a
/// plain managed method: with the default null delegate type, hostfxr requires
/// [UnmanagedCallersOnly], but CoreCLR then builds a managed delegate to the method — which is
/// illegal for UnmanagedCallersOnly methods and either fails fast the process or escapes into the
/// bootstrapper as an SEHException.
/// </summary>
[SuppressMessage( "ReSharper", "UnusedMember.Global", Justification = "Used by HostFxr to map the entrypoint")]
public delegate int ComponentEntryPoint( IntPtr pluginHeaderPtr, int sizeBytes );

/// <summary>
/// Native entry point called by ClassicAssist.Proxy (net472) after starting the
/// .NET runtime in-process.
/// </summary>
/// <remarks>
/// This can be removed as soon as ClassicUO is able to load a dotnetcore Assistant.Engine.
/// </remarks>
public static unsafe class EngineCore
{
    public static int Install( IntPtr pluginHeaderPtr, int sizeBytes )
    {
        try
        {
            Assembly.LoadFrom("ClassicUO.exe" );
            Engine.Install( (PluginHeader*) pluginHeaderPtr );
        }
        catch ( Exception e )
        {
            // Never let an exception cross the native thunk into the .NET Framework
            // bootstrapper — it would fail-fast the whole client.
            try
            {
                SentrySdk.CaptureException( e );
            }
            catch
            {
                // Sentry must not take the client down either.
            }

            try
            {
                // ReSharper disable once LocalizableElement
                Console.WriteLine( $"ERROR: {e}" );
            }
            catch
            {
                // No console attached — ignore.
            }

            Engine.ShowErrorMessageBox( e.ToString() );

            return -1;
        }

        return 0;
    }

}
