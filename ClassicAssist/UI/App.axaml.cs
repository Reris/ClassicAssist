using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ClassicAssist.UI.Views;

namespace ClassicAssist.UI;

public class App : Application
{
    private static Action _onMainClosed;
    private static Task _loading;

    public static Window MainWindow { get; private set; }

    public static void Configure(Action onMainClosed, Task loading)
    {
        _onMainClosed = onMainClosed;
        _loading = loading;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load( this );
    }

    public override void OnFrameworkInitializationCompleted()
    {
        MainWindow = new SplashWindow( () =>
        {
            MainWindow = new MainWindow();
            MainWindow.Closed += ( _, _ ) => { _onMainClosed(); };

            MainWindow.Show();
            MainWindow.Focus();
        }, _loading );

        MainWindow.Show();

        base.OnFrameworkInitializationCompleted();
    }
}