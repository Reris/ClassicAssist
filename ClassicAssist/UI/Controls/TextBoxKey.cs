using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ClassicAssist.Data.Hotkeys;
using static ClassicAssist.Misc.SDLKeys;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.UI.Controls
{
    /// <summary>
    ///     A read-only <see cref="TextBox" /> that captures a key, mouse-button or mouse-wheel shortcut
    ///     and exposes it through the <see cref="Shortcut" /> property.
    /// </summary>
    public class TextBoxKey : TextBox
    {
        public static readonly Avalonia.StyledProperty<ShortcutKeys> ShortcutProperty =
        AvaloniaCompat.RegisterCompat<TextBoxKey, ShortcutKeys>(nameof( Shortcut ), default( ShortcutKeys ), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, onChanged: OnShortcutChanged);

        private readonly KeyModifiers[] _modifierKeys =
        {
            KeyModifiers.Control, KeyModifiers.Shift, KeyModifiers.Alt, KeyModifiers.Meta
        };

        public TextBoxKey()
        {
            IsReadOnly = true;
            IsUndoEnabled = false;

            AddHandler( KeyDownEvent, OnPreviewKeyDownHandler, RoutingStrategies.Tunnel );
            AddHandler( PointerPressedEvent, OnPreviewMouseDownHandler, RoutingStrategies.Tunnel );
            AddHandler( PointerWheelChangedEvent, OnMouseWheelHandler, RoutingStrategies.Tunnel );
        }

        public Key Key { get; private set; }
        public ModKey Modifier { get; set; }

        public ShortcutKeys Shortcut
        {
            get => (ShortcutKeys) GetValue( ShortcutProperty );
            set => SetValue( ShortcutProperty, value );
        }

        private static void OnShortcutChanged( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
            if ( d is TextBoxKey textBoxKey )
            {
                textBoxKey.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private ModKey CheckModifiers( KeyModifiers keyModifiers )
        {
            ModKey keymod = ModKey.None;

            if ( keyModifiers.HasFlag( KeyModifiers.Control ) )
            {
                keymod |= ModKey.LeftCtrl;
            }

            if ( keyModifiers.HasFlag( KeyModifiers.Shift ) )
            {
                keymod |= ModKey.LeftShift;
            }

            if ( keyModifiers.HasFlag( KeyModifiers.Alt ) )
            {
                keymod |= ModKey.LeftAlt;
            }

            if ( keyModifiers.HasFlag( KeyModifiers.Meta ) )
            {
                keymod |= ModKey.RightCtrl;
            }

            return keymod;
        }

        private void OnPreviewKeyDownHandler( object sender, KeyEventArgs e )
        {
            e.Handled = true;

            Key key = e.Key;

            if ( key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftShift || key == Key.RightShift ||
                 key == Key.LWin || key == Key.RWin || key == Key.LeftAlt || key == Key.RightAlt )
            {
                return;
            }

            Modifier = CheckModifiers( e.KeyModifiers );

            Key = key;

            Shortcut = new ShortcutKeys { Modifier = Modifier, Key = Key };
        }

        private void OnPreviewMouseDownHandler( object sender, PointerPressedEventArgs e )
        {
            PointerUpdateKind button = e.GetCurrentPoint( this ).Properties.PointerUpdateKind;

            if ( button == PointerUpdateKind.LeftButtonPressed || button == PointerUpdateKind.RightButtonPressed )
            {
                return;
            }

            e.Handled = true;

            Modifier = CheckModifiers( e.KeyModifiers );

            MouseOptions mouseOption = button switch
            {
                PointerUpdateKind.MiddleButtonPressed => MouseOptions.MiddleButton,
                PointerUpdateKind.XButton1Pressed => MouseOptions.XButton1,
                PointerUpdateKind.XButton2Pressed => MouseOptions.XButton2,
                _ => MouseOptions.None
            };

            Shortcut = new ShortcutKeys { Modifier = Modifier, Mouse = mouseOption };
        }

        private void OnMouseWheelHandler( object sender, PointerWheelEventArgs e )
        {
            e.Handled = true;

            Modifier = CheckModifiers( e.KeyModifiers );

            Shortcut = e.Delta.Y < 0
                ? new ShortcutKeys { Mouse = MouseOptions.MouseWheelDown, Modifier = Modifier }
                : new ShortcutKeys { Mouse = MouseOptions.MouseWheelUp, Modifier = Modifier };
        }
    }
}