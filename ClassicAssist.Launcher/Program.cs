using System;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using ClassicAssist.Launcher;

namespace ClassicAssist.Launcher
{
    internal static class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main( string[] args )
        {
            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime( args );
            }
            catch ( Exception )
            {
                throw;
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With( new Win32PlatformOptions() )
                .With( new X11PlatformOptions() )
                .With( new AvaloniaNativePlatformOptions() )
                .LogToTrace();
    }
}
