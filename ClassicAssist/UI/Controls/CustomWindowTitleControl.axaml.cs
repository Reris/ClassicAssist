using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.Shared.UI;
using ClassicAssist.Shared.UI.Compat;
using System.Windows.Input;

namespace ClassicAssist.UI.Controls
{
    /// <summary>
    ///     Interaction logic for CustomWindowTitleControl.xaml
    /// </summary>
    public partial class CustomWindowTitleControl : UserControl
    {
        public static readonly Avalonia.StyledProperty<object> AdditionalContentProperty =
        AvaloniaCompat.RegisterCompat<CustomWindowTitleControl, object>(nameof( AdditionalContent ), null);

        public static readonly Avalonia.StyledProperty<object> AdditionalButtonsProperty =
        AvaloniaCompat.RegisterCompat<CustomWindowTitleControl, object>(nameof( AdditionalButtons ), null);

        public static readonly Avalonia.StyledProperty<string> CustomTitleProperty =
        AvaloniaCompat.RegisterCompat<CustomWindowTitleControl, string>(nameof( CustomTitle ), default( string ), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly Avalonia.StyledProperty<bool> CanCloseProperty =
        AvaloniaCompat.RegisterCompat<CustomWindowTitleControl, bool>(nameof( CanClose ), true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly Avalonia.StyledProperty<bool> CanMinimizeProperty =
        AvaloniaCompat.RegisterCompat<CustomWindowTitleControl, bool>(nameof( CanMinimize ), true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly Avalonia.StyledProperty<bool> CanMaxmizeProperty =
        AvaloniaCompat.RegisterCompat<CustomWindowTitleControl, bool>(nameof( CanMaximize ), true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly Avalonia.StyledProperty<ICommand> MinimizeCommandProperty =
        AvaloniaCompat.RegisterCompat<CustomWindowTitleControl, ICommand>(nameof( MinimizeCommand ), default);

        private ICommand _maximizeCommand;

        public CustomWindowTitleControl()
        {
            InitializeComponent();
        }

        public object AdditionalButtons
        {
            get => GetValue( AdditionalButtonsProperty );
            set => SetValue( AdditionalButtonsProperty, value );
        }

        public object AdditionalContent
        {
            get => GetValue( AdditionalContentProperty );
            set => SetValue( AdditionalContentProperty, value );
        }

        public bool CanClose
        {
            get => (bool) GetValue( CanCloseProperty );
            set => SetValue( CanCloseProperty, value );
        }

        public bool CanMaximize
        {
            get => (bool) GetValue( CanMaxmizeProperty );
            set => SetValue( CanMaxmizeProperty, value );
        }

        public bool CanMinimize
        {
            get => (bool) GetValue( CanMinimizeProperty );
            set => SetValue( CanMinimizeProperty, value );
        }

        public string CustomTitle
        {
            get => (string) GetValue( CustomTitleProperty );
            set => SetValue( CustomTitleProperty, value );
        }

        public ICommand MaximizeCommand =>
            _maximizeCommand ?? ( _maximizeCommand = new RelayCommand( Maximize, o => true ) );

        public ICommand MinimizeCommand
        {
            get => (ICommand) GetValue( MinimizeCommandProperty );
            set => SetValue( MinimizeCommandProperty, value );
        }

        private static void Maximize( object obj )
        {
            if ( !( obj is Visual visual ) )
            {
                return;
            }

            Window window = TopLevel.GetTopLevel( visual ) as Window;

            if ( window == null )
            {
                return;
            }

            window.WindowState =
                window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }
}