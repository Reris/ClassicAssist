using System;

using Avalonia;

namespace ClassicAssist.Updater
{
    internal static class Program
    {
        [STAThread]
        private static void Main( string[] args )
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime( args );
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
        }
    }
}