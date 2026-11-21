using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for HotkeysTab.xaml
    /// </summary>
    public partial class HotkeysTabControl : UserControl
    {
        public HotkeysTabControl()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseWheel( object sender, PointerWheelEventArgs e )
        {
            if ( e.Handled )
            {
                return;
            }

            e.Handled = true;

            PointerWheelEventArgs eventArg = new PointerWheelEventArgs( sender, e.Pointer, (Visual) sender, default,
                e.Timestamp, e.Properties, e.KeyModifiers, e.Delta )
            {
                RoutedEvent = InputElement.PointerWheelChangedEvent,
                Source = sender
            };

            ScrollViewer parent = GetScrollViewer( (Control) sender );

            parent?.RaiseEvent( eventArg );
        }

        public static ScrollViewer GetScrollViewer( Visual depObj )
        {
            if ( depObj is ScrollViewer scrollViewer )
            {
                return scrollViewer;
            }

            foreach ( Visual child in depObj.GetVisualChildren() )
            {
                ScrollViewer result = GetScrollViewer( child );

                if ( result != null )
                {
                    return result;
                }
            }

            return null;
        }
    }
}