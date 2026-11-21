using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.Shared.UI;
using ClassicAssist.Shared.UI.Compat;
using System.Windows.Input;

namespace ClassicAssist.Controls
{
    /// <summary>
    ///     Interaction logic for FilterControl.xaml
    /// </summary>
    public partial class FilterControl : UserControl
    {
        public static readonly Avalonia.StyledProperty<string> FilterTextProperty =
        AvaloniaCompat.RegisterCompat<FilterControl, string>(nameof( FilterText ), default( string ), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly Avalonia.StyledProperty<bool> IsFilterVisibleProperty =
        AvaloniaCompat.RegisterCompat<FilterControl, bool>(nameof( IsFilterVisible ), default( bool ), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly Avalonia.StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaCompat.RegisterCompat<FilterControl, bool>(nameof( ShowCloseButton ), true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        private ICommand _closeCommand;

        public FilterControl()
        {
            InitializeComponent();
        }

        public ICommand CloseCommand => _closeCommand ?? ( _closeCommand = new RelayCommand( Close ) );

        public string FilterText
        {
            get => (string)GetValue( FilterTextProperty );
            set => SetValue( FilterTextProperty, value );
        }

        public bool IsFilterVisible
        {
            get => (bool)GetValue( IsFilterVisibleProperty );
            set => SetValue( IsFilterVisibleProperty, value );
        }

        public bool ShowCloseButton
        {
            get => (bool)GetValue( ShowCloseButtonProperty );
            set => SetValue( ShowCloseButtonProperty, value );
        }

        private void Close( object obj )
        {
            FilterText = string.Empty;
            IsFilterVisible = false;
        }
    }
}