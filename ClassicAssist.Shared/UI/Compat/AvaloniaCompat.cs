using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Styling;

namespace ClassicAssist.Shared.UI.Compat
{
    /// <summary>
    ///     Compatibility helpers emulating WPF DependencyProperty registration semantics on Avalonia.
    /// </summary>
    public static class AvaloniaCompat
    {
        public static StyledProperty<TValue> RegisterCompat<TOwner, TValue>( string name,
            TValue defaultValue = default, bool inherits = false,
            BindingMode defaultBindingMode = BindingMode.OneWay,
            Action<AvaloniaObject, AvaloniaPropertyChangedEventArgs> onChanged = null )
            where TOwner : AvaloniaObject
        {
            StyledProperty<TValue> property = AvaloniaProperty.Register<TOwner, TValue>( name, defaultValue,
                inherits, defaultBindingMode );

            if ( onChanged != null )
            {
                property.Changed.AddClassHandler<TOwner, TValue>( ( owner, e ) =>
                    onChanged( owner, e ) );
            }

            return property;
        }

        public static AttachedProperty<TValue> RegisterAttachedCompat<TOwner, TValue>( string name,
            TValue defaultValue = default, bool inherits = false,
            BindingMode defaultBindingMode = BindingMode.OneWay,
            Action<AvaloniaObject, AvaloniaPropertyChangedEventArgs> onChanged = null )
            where TOwner : AvaloniaObject
        {
            AttachedProperty<TValue> property = AvaloniaProperty.RegisterAttached<TOwner, TValue>( name,
                typeof( TOwner ), defaultValue, inherits, defaultBindingMode );

            if ( onChanged != null )
            {
                property.Changed.AddClassHandler<TOwner, TValue>( ( owner, e ) =>
                    onChanged( owner, e ) );
            }

            return property;
        }
    }
}
