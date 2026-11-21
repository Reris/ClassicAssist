#region License

// Copyright (C) 2020 Reetus
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

//https://stackoverflow.com/questions/4305565/wpf-context-menu-on-left-click
namespace ClassicAssist.UI.Misc.Behaviours
{
    public class ClickOpensContextMenuBehaviour
    {
        private static readonly Avalonia.StyledProperty<bool> ClickOpensContextMenuProperty =
            AvaloniaProperty.RegisterAttached<ClickOpensContextMenuBehaviour, AvaloniaObject, bool>( "Enabled" );

        static ClickOpensContextMenuBehaviour()
        {
            ClickOpensContextMenuProperty.Changed.AddClassHandler<AvaloniaObject, bool>( HandlePropertyChanged );
        }

        public static bool GetEnabled( AvaloniaObject obj )
        {
            return (bool) obj.GetValue( ClickOpensContextMenuProperty );
        }

        public static void SetEnabled( AvaloniaObject obj, bool value )
        {
            obj.SetValue( ClickOpensContextMenuProperty, value );
        }

        private static void HandlePropertyChanged( AvaloniaObject obj, AvaloniaPropertyChangedEventArgs args )
        {
            if ( obj is Image image )
            {
                image.PointerPressed -= ExecuteMouseDown;
                image.PointerPressed += ExecuteMouseDown;
            }
        }

        private static void ExecuteMouseDown( object sender, PointerPressedEventArgs args )
        {
            if ( !( sender is AvaloniaObject obj ) )
            {
                return;
            }

            bool enabled = (bool) obj.GetValue( ClickOpensContextMenuProperty );

            if ( !enabled )
            {
                return;
            }

            Image image = sender as Image;

            if ( image?.ContextMenu != null )
            {
                image.ContextMenu.Open();
            }
        }
    }
}
