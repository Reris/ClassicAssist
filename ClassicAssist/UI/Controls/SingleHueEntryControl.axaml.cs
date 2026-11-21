using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Controls.Shapes;
using ClassicAssist.UO.Data;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.UI.Controls
{
    /// <summary>
    ///     Interaction logic for HueEntryControl.xaml
    /// </summary>
    public partial class SingleHueEntryControl : UserControl
    {
        public static readonly Avalonia.StyledProperty<HueEntry> HueEntryProperty =
        AvaloniaCompat.RegisterCompat<SingleHueEntryControl, HueEntry>(nameof( HueEntry ), default( HueEntry ), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, onChanged: PropertyChangedCallback);

        public SingleHueEntryControl()
        {
            InitializeComponent();
        }

        public HueEntry HueEntry
        {
            get => (HueEntry) GetValue( HueEntryProperty );
            set => SetValue( HueEntryProperty, value );
        }

        private static void PropertyChangedCallback( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
            if ( d is SingleHueEntryControl control )
            {
                control.SetHues( (HueEntry) e.NewValue );
            }
        }

        private void SetHues( HueEntry entry )
        {
            // ClassicUO uses ColorTable index 8 as a hue's single representative colour
            // ( HuesLoader.GetUnicodeFontColor, the shade used to render solid hued text ).
            SolidColorBrush brush = new SolidColorBrush( Convert555ToARGB( entry.Colors[8] ) );

            StackPanel.Children.Add( new Rectangle { Fill = brush, Width = 320 } );
        }

        protected static Color Convert555ToARGB( short Col )
        {
            int r = ( (short) ( Col >> 10 ) & 31 ) * 8;
            int g = ( (short) ( Col >> 5 ) & 31 ) * 8;
            int b = ( Col & 31 ) * 8;
            return Color.FromArgb( 255, (byte) r, (byte) g, (byte) b );
        }
    }
}