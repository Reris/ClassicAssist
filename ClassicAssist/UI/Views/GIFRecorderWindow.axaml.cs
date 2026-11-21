using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.UI.ViewModels;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for GIFRecorderWindow.axaml
    /// </summary>
    public partial class GIFRecorderWindow : Window
    {
        public GIFRecorderWindow()
        {
            InitializeComponent();
            DataContext = new GIFRecorderViewModel( this );
        }
    }
}