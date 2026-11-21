using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;


namespace ClassicAssist.Updater
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnRequestNavigate( object sender, RoutedEventArgs e )
        {
            if ( sender is Button button && button.Tag is string url )
            {
                Process.Start( new ProcessStartInfo( url ) { UseShellExecute = true } );
            }
        }
    }
}