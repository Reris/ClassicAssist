using Avalonia;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Misc
{
    /// <summary>
    ///     Binding proxy for accessing DataContext from outside the visual tree.
    /// </summary>
    public class BindingProxy : AvaloniaObject
    {
        public static readonly Avalonia.StyledProperty<object> DataProperty =
        AvaloniaCompat.RegisterCompat<BindingProxy, object>("Data", null);

        public object Data
        {
            get => GetValue( DataProperty );
            set => SetValue( DataProperty, value );
        }
    }
}