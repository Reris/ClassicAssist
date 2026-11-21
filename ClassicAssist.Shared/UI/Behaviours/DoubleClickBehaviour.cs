using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace ClassicAssist.Shared.UI.Behaviours
{
    /// <summary>
    ///     Replacement for WPF MouseBinding (LeftDoubleClick) - executes a command on double click.
    /// </summary>
    public class DoubleClickBehaviour : Behavior<InputElement>
    {
        public static readonly StyledProperty<ICommand> CommandProperty =
            AvaloniaProperty.Register<DoubleClickBehaviour, ICommand>( nameof( Command ) );

        public static readonly StyledProperty<object> CommandParameterProperty =
            AvaloniaProperty.Register<DoubleClickBehaviour, object>( nameof( CommandParameter ) );

        public ICommand Command
        {
            get => GetValue( CommandProperty );
            set => SetValue( CommandProperty, value );
        }

        public object CommandParameter
        {
            get => GetValue( CommandParameterProperty );
            set => SetValue( CommandParameterProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.DoubleTapped += OnDoubleTapped;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.DoubleTapped -= OnDoubleTapped;
        }

        private void OnDoubleTapped( object sender, TappedEventArgs e )
        {
            ICommand command = Command;

            command?.Execute( CommandParameter );
        }
    }
}
