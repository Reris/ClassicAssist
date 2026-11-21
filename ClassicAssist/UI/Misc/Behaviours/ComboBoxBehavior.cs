#region License

// Copyright (C) 2025 Reetus
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using ClassicAssist.Shared.UI.Compat;
using System.Windows.Input;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class ComboBoxBehavior : Behavior<ComboBox>
    {
        public static readonly Avalonia.StyledProperty<ICommand> CommandBindingProperty =
        AvaloniaCompat.RegisterCompat<ComboBoxBehavior, ICommand>(nameof( CommandBinding ), null, onChanged: PropertyChangedCallback);

        public static readonly Avalonia.StyledProperty<object> CommandParameterProperty =
        AvaloniaCompat.RegisterCompat<ComboBoxBehavior, object>(nameof( CommandParameter ), null, onChanged: PropertyChangedCallback);

        public static readonly Avalonia.StyledProperty<bool> OnlyUserTriggeredProperty =
        AvaloniaCompat.RegisterCompat<ComboBoxBehavior, bool>(nameof( OnlyUserTriggered ), false, onChanged: PropertyChangedCallback);

        private bool _userTriggered;

        public ICommand CommandBinding
        {
            get => (ICommand) GetValue( CommandBindingProperty );
            set => SetValue( CommandBindingProperty, value );
        }

        public object CommandParameter
        {
            get => GetValue( CommandParameterProperty );
            set => SetValue( CommandParameterProperty, value );
        }

        public bool OnlyUserTriggered
        {
            get => (bool) GetValue( OnlyUserTriggeredProperty );
            set => SetValue( OnlyUserTriggeredProperty, value );
        }

        private static void PropertyChangedCallback( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnSelectionChanged;
            AssociatedObject.AddHandler( InputElement.PointerPressedEvent, OnPreviewMouseDown, RoutingStrategies.Tunnel );
        }

        private void OnPreviewMouseDown( object sender, PointerPressedEventArgs e )
        {
            _userTriggered = true;
        }

        private void OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if ( OnlyUserTriggered && !_userTriggered )
            {
                return;
            }

            CommandBinding?.Execute( AssociatedObject.SelectedItem );
            _userTriggered = false;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
            AssociatedObject.RemoveHandler( InputElement.PointerPressedEvent, OnPreviewMouseDown );
        }
    }
}