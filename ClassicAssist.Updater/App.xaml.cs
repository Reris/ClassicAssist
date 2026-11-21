using System;
using System.IO;
using System.Threading;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using ClassicAssist.Shared;
using ClassicAssist.Updater.Properties;
using CommandLine;
using Exceptionless;
using IOPath = System.IO.Path;

namespace ClassicAssist.Updater
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static CommandLineOptions CurrentOptions { get; set; }

        public static UpdaterSettings UpdaterSettings { get; set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load( this );
        }

        public override void OnFrameworkInitializationCompleted()
        {
            string[] args = Array.Empty<string>();

            if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                args = desktop.Args ?? args;
            }

            if ( !string.IsNullOrEmpty( Settings.Default.ExceptionlessKey ) )
            {
                ExceptionlessClient.Default.Configuration.DefaultData.Add( "Locale",
                    Thread.CurrentThread.CurrentUICulture.Name );
                ExceptionlessClient.Default.Startup( Settings.Default.ExceptionlessKey );
            }

            Parser.Default.ParseArguments<CommandLineOptions>( args ).WithParsed( o => CurrentOptions = o );

            if ( string.IsNullOrEmpty( CurrentOptions.Path ) )
            {
                CurrentOptions.Path = Environment.CurrentDirectory;
            }

            UpdaterSettings = UpdaterSettings.Load( CurrentOptions.Path ?? Environment.CurrentDirectory );

            if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime )
            {
                desktopLifetime.ShutdownRequested += ( s, ea ) =>
                    UpdaterSettings.Save( UpdaterSettings, CurrentOptions.Path ?? Environment.CurrentDirectory );

                if ( CurrentOptions.CurrentVersion != null )
                {
                    desktopLifetime.Shutdown();
                    return;
                }

                string dllPath = IOPath.Combine( CurrentOptions.Path, "ClassicAssist.dll" );

                try
                {
                    CurrentOptions.CurrentVersion = VersionHelpers.GetProductVersion( dllPath ).ToString();
                }
                catch ( Exception )
                {
                    CurrentOptions.Force = true;
                }

                desktopLifetime.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}