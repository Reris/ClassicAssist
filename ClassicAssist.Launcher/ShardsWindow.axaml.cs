using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;


namespace ClassicAssist.Launcher
{
    /// <summary>
    ///     Interaction logic for ShardsWindow.axaml
    /// </summary>
    public partial class ShardsWindow : Window
    {
        public ShardsWindow()
        {
            InitializeComponent();
        }

        private void GitHubIssueLink_OnClick( object sender, RoutedEventArgs e )
        {
            Process.Start( new ProcessStartInfo { FileName = "https://github.com/Reetus/ClassicAssist-Shards/issues", UseShellExecute = true } );
        }
    }
}