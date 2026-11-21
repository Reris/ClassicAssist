using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

using System.Xml;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

namespace ClassicAssist.Browser
{
    /// <summary>
    ///     Interaction logic for MainWindow.axaml
    /// </summary>
    public partial class MacroBrowserControl : UserControl
    {
        private const string CONTRIBUTE_URL = "https://github.com/Reetus/ClassicAssist-Macros/blob/master/README.md";

        public MacroBrowserControl()
        {
            InitializeComponent();

            CodeTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader( Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ),
                    "Python.Dark.xshd" ) ), HighlightingManager.Instance );
        }

        private void Hyperlink_Click( object sender, RoutedEventArgs e )
        {
            Process.Start( new ProcessStartInfo { FileName = CONTRIBUTE_URL, UseShellExecute = true } );
        }
    }
}