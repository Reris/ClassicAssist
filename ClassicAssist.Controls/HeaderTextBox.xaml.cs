using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Controls
{
    /// <summary>
    ///     Interaction logic for HeaderTextBox.xaml
    /// </summary>
    public partial class HeaderTextBox : UserControl
    {
        public static readonly Avalonia.StyledProperty<object> HeaderProperty =
        AvaloniaCompat.RegisterCompat<HeaderTextBox, object>(nameof( Header ), null, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly Avalonia.StyledProperty<object> ValueProperty =
        AvaloniaCompat.RegisterCompat<HeaderTextBox, object>(nameof( Value ), string.Empty, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public HeaderTextBox()
        {
            InitializeComponent();
        }

        public object Header
        {
            get => GetValue( HeaderProperty );
            set => SetValue( HeaderProperty, value );
        }

        public object Value
        {
            get => GetValue( ValueProperty );
            set => SetValue( ValueProperty, value );
        }
    }
}