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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using ClassicAssist.Shared.UI.Compat;
using System.Windows.Input;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class ClipboardBehaviour : Behavior<Control>
    {
        public static readonly Avalonia.StyledProperty<ICommand> CopyCommandProperty =
        AvaloniaCompat.RegisterCompat<ClipboardBehaviour, ICommand>(nameof( CopyCommand ), null);

        public static readonly Avalonia.StyledProperty<ICommand> PasteCommandProperty =
        AvaloniaCompat.RegisterCompat<ClipboardBehaviour, ICommand>(nameof( PasteCommand ), null);

        public static readonly Avalonia.StyledProperty<object> CommandParameterProperty =
        AvaloniaCompat.RegisterCompat<ClipboardBehaviour, object>(nameof( CommandParameter ), null);

        public object CommandParameter
        {
            get => GetValue( CommandParameterProperty );
            set => SetValue( CommandParameterProperty, value );
        }

        public ICommand CopyCommand
        {
            get => (ICommand) GetValue( CopyCommandProperty );
            set => SetValue( CopyCommandProperty, value );
        }

        public ICommand PasteCommand
        {
            get => (ICommand) GetValue( PasteCommandProperty );
            set => SetValue( PasteCommandProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AddHandler( InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel );
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler( InputElement.KeyDownEvent, OnKeyDown );

            base.OnDetaching();
        }

        private void OnKeyDown( object sender, KeyEventArgs e )
        {
            if ( e.KeyModifiers.HasFlag( KeyModifiers.Control ) )
            {
                if ( e.Key == Key.C && CopyCommand?.CanExecute( CommandParameter ) == true )
                {
                    CopyCommand.Execute( CommandParameter );
                    e.Handled = true;
                }
                else if ( e.Key == Key.V && PasteCommand?.CanExecute( CommandParameter ) == true )
                {
                    PasteCommand.Execute( CommandParameter );
                    e.Handled = true;
                }
            }
        }
    }
}