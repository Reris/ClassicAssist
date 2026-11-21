using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Threading;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Controls
{
    /// <summary>
    ///     Interaction logic for EditTextBlock.xaml
    /// </summary>
    public partial class EditTextBlock : UserControl
    {
        public static readonly Avalonia.StyledProperty<string> TextProperty =
        AvaloniaCompat.RegisterCompat<EditTextBlock, string>(nameof( Text ), null, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly Avalonia.StyledProperty<string> LabelProperty =
        AvaloniaCompat.RegisterCompat<EditTextBlock, string>(nameof( Label ), null);

        public static readonly Avalonia.StyledProperty<bool> ShowIconProperty =
        AvaloniaCompat.RegisterCompat<EditTextBlock, bool>(nameof( ShowIcon ), default);

        public static readonly Avalonia.StyledProperty<bool> CanEditProperty =
        AvaloniaCompat.RegisterCompat<EditTextBlock, bool>(nameof( CanEdit ), true);

        public static readonly Avalonia.StyledProperty<object> ButtonsProperty =
        AvaloniaCompat.RegisterCompat<EditTextBlock, object>(nameof( Buttons ), default);

        public EditTextBlock()
        {
            InitializeComponent();
        }

        public object Buttons
        {
            get => GetValue( ButtonsProperty );
            set => SetValue( ButtonsProperty, value );
        }

        public bool CanEdit
        {
            get => (bool) GetValue( CanEditProperty );
            set => SetValue( CanEditProperty, value );
        }

        public string Label
        {
            get => (string) GetValue( LabelProperty );
            set => SetValue( LabelProperty, value );
        }

        public bool ShowIcon
        {
            get => (bool) GetValue( ShowIconProperty );
            set => SetValue( ShowIconProperty, value );
        }

        public string Text
        {
            get => (string) GetValue( TextProperty );
            set => SetValue( TextProperty, value );
        }

        private void TextBlock_OnMouseDown( object sender, PointerPressedEventArgs e )
        {
            if ( e.ClickCount <= 1 )
            {
                return;
            }

            if ( !CanEdit )
            {
                return;
            }

            ShowTextBox();
        }

        private void ShowTextBox()
        {
            textBlock.IsVisible = false;
            pencilButton.IsVisible = false;
            buttonsPanel.IsVisible = false;
            textBox.IsVisible = true;
            textBox.CaretIndex = textBox.Text.Length;
            textBox.SelectAll();

            Dispatcher.UIThread.Post( () => textBox.Focus(), DispatcherPriority.Render );
        }

        private void TextBox_OnKeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key == Key.Return )
            {
                TextBox_LostFocus( sender, null );
            }
        }

        private void TextBox_LostFocus( object sender, RoutedEventArgs e )
        {
            textBlock.IsVisible = true;
            pencilButton.IsVisible = ShowIcon;
            buttonsPanel.IsVisible = true;
            ( (TextBox) sender ).IsVisible = false;
        }

        private void Button_OnClick( object sender, RoutedEventArgs e )
        {
            ShowTextBox();
        }
    }
}