#region License

// Copyright (C) 2026 Reetus
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Styling;

namespace ClassicAssist.Shared.UI.Compat
{
    /// <summary>
    ///     Emulates WPF DataTriggers / MultiDataTriggers on Avalonia. Up to eight independent
    ///     conditions may be attached to an element. Each condition ( Bind/Value/Key,
    ///     Bind2/Value2/Key2, ... ) binds a value, compares it against an expected value and,
    ///     when it matches, applies the class <c>ca-&lt;Key&gt;</c> to the target element so that
    ///     a style with selector <c>^.ca-&lt;Key&gt;</c> can react to it. Conditions are evaluated
    ///     independently - use a separate condition ( with its own key ) for each class.
    /// </summary>
    public static class TriggerCondition
    {
        private const int CONDITION_COUNT = 8;

        public static readonly AttachedProperty<BindingBase> BindProperty =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, BindingBase>( "Bind", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> ValueProperty =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "Value", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<string> KeyProperty =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, string>( "Key", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<BindingBase> Bind2Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, BindingBase>( "Bind2", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> Value2Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "Value2", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<string> Key2Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, string>( "Key2", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<BindingBase> Bind3Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, BindingBase>( "Bind3", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> Value3Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "Value3", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<string> Key3Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, string>( "Key3", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<BindingBase> Bind4Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, BindingBase>( "Bind4", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> Value4Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "Value4", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<string> Key4Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, string>( "Key4", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<BindingBase> Bind5Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, BindingBase>( "Bind5", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> Value5Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "Value5", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<string> Key5Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, string>( "Key5", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<BindingBase> Bind6Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, BindingBase>( "Bind6", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> Value6Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "Value6", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<string> Key6Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, string>( "Key6", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<BindingBase> Bind7Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, BindingBase>( "Bind7", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> Value7Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "Value7", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<string> Key7Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, string>( "Key7", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<BindingBase> Bind8Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, BindingBase>( "Bind8", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> Value8Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "Value8", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<string> Key8Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, string>( "Key8", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> CurrentValueProperty =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "CurrentValue", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> CurrentValue2Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "CurrentValue2", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> CurrentValue3Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "CurrentValue3", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> CurrentValue4Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "CurrentValue4", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> CurrentValue5Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "CurrentValue5", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> CurrentValue6Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "CurrentValue6", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> CurrentValue7Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "CurrentValue7", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> CurrentValue8Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "CurrentValue8", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<IDisposable> SubscriptionProperty =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, IDisposable>( "Subscription", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<IDisposable> Subscription2Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, IDisposable>( "Subscription2", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<IDisposable> Subscription3Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, IDisposable>( "Subscription3", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<IDisposable> Subscription4Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, IDisposable>( "Subscription4", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<IDisposable> Subscription5Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, IDisposable>( "Subscription5", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<IDisposable> Subscription6Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, IDisposable>( "Subscription6", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<IDisposable> Subscription7Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, IDisposable>( "Subscription7", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<IDisposable> Subscription8Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, IDisposable>( "Subscription8", typeof( TriggerCondition ) );

        private static readonly AvaloniaProperty[] BIND_PROPERTIES =
        {
            BindProperty, Bind2Property, Bind3Property, Bind4Property, Bind5Property, Bind6Property, Bind7Property,
            Bind8Property
        };

        private static readonly AvaloniaProperty[] VALUE_PROPERTIES =
        {
            ValueProperty, Value2Property, Value3Property, Value4Property, Value5Property, Value6Property,
            Value7Property, Value8Property
        };

        private static readonly AvaloniaProperty[] KEY_PROPERTIES =
        {
            KeyProperty, Key2Property, Key3Property, Key4Property, Key5Property, Key6Property, Key7Property,
            Key8Property
        };

        private static readonly AvaloniaProperty[] CURRENT_VALUE_PROPERTIES =
        {
            CurrentValueProperty, CurrentValue2Property, CurrentValue3Property, CurrentValue4Property,
            CurrentValue5Property, CurrentValue6Property, CurrentValue7Property, CurrentValue8Property
        };

        private static readonly AvaloniaProperty[] SUBSCRIPTION_PROPERTIES =
        {
            SubscriptionProperty, Subscription2Property, Subscription3Property, Subscription4Property,
            Subscription5Property, Subscription6Property, Subscription7Property, Subscription8Property
        };

        static TriggerCondition()
        {
            for ( int i = 0; i < CONDITION_COUNT; i++ )
            {
                BIND_PROPERTIES[i].Changed.AddClassHandler<AvaloniaObject>( OnBindChanged );
                VALUE_PROPERTIES[i].Changed.AddClassHandler<AvaloniaObject>( OnConditionChanged );
                KEY_PROPERTIES[i].Changed.AddClassHandler<AvaloniaObject>( OnConditionChanged );
                CURRENT_VALUE_PROPERTIES[i].Changed.AddClassHandler<AvaloniaObject>( OnConditionChanged );
            }
        }

        public static void SetBind( AvaloniaObject element, BindingBase value ) => element.SetValue( BindProperty, value );

        public static BindingBase GetBind( AvaloniaObject element ) => element.GetValue( BindProperty );

        public static void SetValue( AvaloniaObject element, object value ) => element.SetValue( ValueProperty, value );

        public static object GetValue( AvaloniaObject element ) => element.GetValue( ValueProperty );

        public static void SetKey( AvaloniaObject element, string value ) => element.SetValue( KeyProperty, value );

        public static string GetKey( AvaloniaObject element ) => element.GetValue( KeyProperty );

        public static void SetBind2( AvaloniaObject element, BindingBase value ) =>
            element.SetValue( Bind2Property, value );

        public static BindingBase GetBind2( AvaloniaObject element ) => element.GetValue( Bind2Property );

        public static void SetValue2( AvaloniaObject element, object value ) =>
            element.SetValue( Value2Property, value );

        public static object GetValue2( AvaloniaObject element ) => element.GetValue( Value2Property );

        public static void SetKey2( AvaloniaObject element, string value ) => element.SetValue( Key2Property, value );

        public static string GetKey2( AvaloniaObject element ) => element.GetValue( Key2Property );

        public static void SetBind3( AvaloniaObject element, BindingBase value ) =>
            element.SetValue( Bind3Property, value );

        public static BindingBase GetBind3( AvaloniaObject element ) => element.GetValue( Bind3Property );

        public static void SetValue3( AvaloniaObject element, object value ) =>
            element.SetValue( Value3Property, value );

        public static object GetValue3( AvaloniaObject element ) => element.GetValue( Value3Property );

        public static void SetKey3( AvaloniaObject element, string value ) => element.SetValue( Key3Property, value );

        public static string GetKey3( AvaloniaObject element ) => element.GetValue( Key3Property );

        public static void SetBind4( AvaloniaObject element, BindingBase value ) =>
            element.SetValue( Bind4Property, value );

        public static BindingBase GetBind4( AvaloniaObject element ) => element.GetValue( Bind4Property );

        public static void SetValue4( AvaloniaObject element, object value ) =>
            element.SetValue( Value4Property, value );

        public static object GetValue4( AvaloniaObject element ) => element.GetValue( Value4Property );

        public static void SetKey4( AvaloniaObject element, string value ) => element.SetValue( Key4Property, value );

        public static string GetKey4( AvaloniaObject element ) => element.GetValue( Key4Property );

        public static void SetBind5( AvaloniaObject element, BindingBase value ) =>
            element.SetValue( Bind5Property, value );

        public static BindingBase GetBind5( AvaloniaObject element ) => element.GetValue( Bind5Property );

        public static void SetValue5( AvaloniaObject element, object value ) =>
            element.SetValue( Value5Property, value );

        public static object GetValue5( AvaloniaObject element ) => element.GetValue( Value5Property );

        public static void SetKey5( AvaloniaObject element, string value ) => element.SetValue( Key5Property, value );

        public static string GetKey5( AvaloniaObject element ) => element.GetValue( Key5Property );

        public static void SetBind6( AvaloniaObject element, BindingBase value ) =>
            element.SetValue( Bind6Property, value );

        public static BindingBase GetBind6( AvaloniaObject element ) => element.GetValue( Bind6Property );

        public static void SetValue6( AvaloniaObject element, object value ) =>
            element.SetValue( Value6Property, value );

        public static object GetValue6( AvaloniaObject element ) => element.GetValue( Value6Property );

        public static void SetKey6( AvaloniaObject element, string value ) => element.SetValue( Key6Property, value );

        public static string GetKey6( AvaloniaObject element ) => element.GetValue( Key6Property );

        public static void SetBind7( AvaloniaObject element, BindingBase value ) =>
            element.SetValue( Bind7Property, value );

        public static BindingBase GetBind7( AvaloniaObject element ) => element.GetValue( Bind7Property );

        public static void SetValue7( AvaloniaObject element, object value ) =>
            element.SetValue( Value7Property, value );

        public static object GetValue7( AvaloniaObject element ) => element.GetValue( Value7Property );

        public static void SetKey7( AvaloniaObject element, string value ) => element.SetValue( Key7Property, value );

        public static string GetKey7( AvaloniaObject element ) => element.GetValue( Key7Property );

        public static void SetBind8( AvaloniaObject element, BindingBase value ) =>
            element.SetValue( Bind8Property, value );

        public static BindingBase GetBind8( AvaloniaObject element ) => element.GetValue( Bind8Property );

        public static void SetValue8( AvaloniaObject element, object value ) =>
            element.SetValue( Value8Property, value );

        public static object GetValue8( AvaloniaObject element ) => element.GetValue( Value8Property );

        public static void SetKey8( AvaloniaObject element, string value ) => element.SetValue( Key8Property, value );

        public static string GetKey8( AvaloniaObject element ) => element.GetValue( Key8Property );

        private static void OnBindChanged( AvaloniaObject element, AvaloniaPropertyChangedEventArgs e )
        {
            int index = Array.IndexOf( BIND_PROPERTIES, e.Property );

            if ( index < 0 )
            {
                return;
            }

            AvaloniaProperty currentProperty = CURRENT_VALUE_PROPERTIES[index];
            AvaloniaProperty subscriptionProperty = SUBSCRIPTION_PROPERTIES[index];

            if ( e.OldValue is BindingBase )
            {
                ( element.GetValue( subscriptionProperty ) as IDisposable )?.Dispose();
                element.SetValue( subscriptionProperty, null );
                element.ClearValue( currentProperty );
            }

            if ( e.NewValue is BindingBase binding )
            {
                IDisposable subscription = element.Bind( currentProperty, binding );
                element.SetValue( subscriptionProperty, subscription );
            }

            Evaluate( element );
        }

        private static void OnConditionChanged( AvaloniaObject element, AvaloniaPropertyChangedEventArgs e )
        {
            Evaluate( element );
        }

        private static void Evaluate( AvaloniaObject element )
        {
            if ( !( element is StyledElement styledElement ) )
            {
                return;
            }

            for ( int i = 0; i < CONDITION_COUNT; i++ )
            {
                string key = element.GetValue( KEY_PROPERTIES[i] ) as string;

                if ( string.IsNullOrEmpty( key ) )
                {
                    continue;
                }

                bool active = Matches( element.GetValue( CURRENT_VALUE_PROPERTIES[i] ), element.GetValue( VALUE_PROPERTIES[i] ) );

                string className = $"ca-{key}";

                if ( active )
                {
                    if ( !styledElement.Classes.Contains( className ) )
                    {
                        styledElement.Classes.Add( className );
                    }
                }
                else
                {
                    styledElement.Classes.Remove( className );
                }
            }
        }

        private static bool Matches( object actual, object expected )
        {
            if ( expected == null && actual == null )
            {
                return true;
            }

            if ( expected == null || actual == null )
            {
                return false;
            }

            if ( Equals( actual, expected ) )
            {
                return true;
            }

            string actualString = actual as string ?? Convert.ToString( actual, CultureInfo.InvariantCulture );
            string expectedString = expected as string ?? Convert.ToString( expected, CultureInfo.InvariantCulture );

            return string.Equals( actualString, expectedString, StringComparison.OrdinalIgnoreCase );
        }
    }
}
