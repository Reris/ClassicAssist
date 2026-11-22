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
    ///     Emulates WPF DataTriggers / MultiDataTriggers on Avalonia. A binding is
    ///     evaluated against an expected value; when it matches, the class
    ///     <c>ca-&lt;Key&gt;</c> is applied to the target element so that a style with
    ///     selector <c>^.ca-&lt;Key&gt;</c> can react to it.
    /// </summary>
    public static class TriggerCondition
    {
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

        public static readonly AttachedProperty<BindingBase> Bind3Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, BindingBase>( "Bind3", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> Value3Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "Value3", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> CurrentValueProperty =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "CurrentValue", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> CurrentValue2Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "CurrentValue2", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<object> CurrentValue3Property =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, object>( "CurrentValue3", typeof( TriggerCondition ) );

        public static readonly AttachedProperty<IDisposable> SubscriptionProperty =
            AvaloniaProperty.RegisterAttached<AvaloniaObject, IDisposable>( "Subscription", typeof( TriggerCondition ) );

        static TriggerCondition()
        {
            BindProperty.Changed.AddClassHandler<AvaloniaObject>( OnBindChanged );
            ValueProperty.Changed.AddClassHandler<AvaloniaObject>( OnConditionChanged );
            KeyProperty.Changed.AddClassHandler<AvaloniaObject>( OnConditionChanged );
            Bind2Property.Changed.AddClassHandler<AvaloniaObject>( OnBindChanged );
            Value2Property.Changed.AddClassHandler<AvaloniaObject>( OnConditionChanged );
            Bind3Property.Changed.AddClassHandler<AvaloniaObject>( OnBindChanged );
            Value3Property.Changed.AddClassHandler<AvaloniaObject>( OnConditionChanged );
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

        public static void SetBind3( AvaloniaObject element, BindingBase value ) =>
            element.SetValue( Bind3Property, value );

        public static BindingBase GetBind3( AvaloniaObject element ) => element.GetValue( Bind3Property );

        public static void SetValue3( AvaloniaObject element, object value ) =>
            element.SetValue( Value3Property, value );

        public static object GetValue3( AvaloniaObject element ) => element.GetValue( Value3Property );

        private static void OnBindChanged( AvaloniaObject element, AvaloniaPropertyChangedEventArgs e )
        {
            AvaloniaProperty currentProperty = e.Property == BindProperty
                ? CurrentValueProperty
                : e.Property == Bind2Property
                    ? CurrentValue2Property
                    : CurrentValue3Property;

            if ( e.OldValue is BindingBase )
            {
                element.GetValue( SubscriptionProperty )?.Dispose();
                element.SetValue( SubscriptionProperty, null );
                element.ClearValue( currentProperty );
            }

            if ( e.NewValue is BindingBase binding )
            {
                IDisposable subscription = element.Bind( currentProperty, binding );
                element.SetValue( SubscriptionProperty, subscription );
            }
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

            string key = element.GetValue( KeyProperty );

            if ( string.IsNullOrEmpty( key ) )
            {
                return;
            }

            bool active = Matches( element.GetValue( CurrentValueProperty ), element.GetValue( ValueProperty ) ) &&
                Matches( element.GetValue( CurrentValue2Property ), element.GetValue( Value2Property ) ) &&
                Matches( element.GetValue( CurrentValue3Property ), element.GetValue( Value3Property ) );

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
