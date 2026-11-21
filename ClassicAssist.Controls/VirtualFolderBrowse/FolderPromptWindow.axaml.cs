using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;

namespace ClassicAssist.Controls.VirtualFolderBrowse
{
    /// <summary>
    ///     Interaction logic for FolderPromptWindow.xaml
    /// </summary>
    public partial class FolderPromptWindow : Window
    {
        public FolderPromptWindow()
        {
            InitializeComponent();
        }

        public string Text { get; set; }

        public bool? DialogResult { get; private set; }

        private void CancelOnClick( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }

        private void OKOnClick( object sender, RoutedEventArgs e )
        {
            DialogResult = true;
            Text = TextBox.Text;
            Close();
        }
    }
}