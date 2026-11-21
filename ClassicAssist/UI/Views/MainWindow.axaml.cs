using Avalonia.Input;
using Assistant;

using Avalonia.Controls;
namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.axaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Engine.KeyboardLayoutId = System.Globalization.CultureInfo.CurrentCulture.KeyboardLayoutId;
        }
    }
}