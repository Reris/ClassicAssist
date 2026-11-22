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

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class WindowMouseDownBehaviour : Behavior<Control>
    {
        private const double MINIMUM_DRAG_DISTANCE = 4;

        private static Point _lastMouseDown;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PointerPressed += OnMouseDown;
            AssociatedObject.PointerMoved += OnMouseMove;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PointerPressed -= OnMouseDown;
            AssociatedObject.PointerMoved -= OnMouseMove;
        }

        private void OnMouseMove( object sender, PointerEventArgs e )
        {
            if ( !( sender is Control element ) )
            {
                return;
            }

            if ( !e.GetCurrentPoint( element ).Properties.IsLeftButtonPressed )
            {
                return;
            }

            Point currentPosition = e.GetPosition( element );

            if ( !( Math.Abs( currentPosition.X - _lastMouseDown.X ) > MINIMUM_DRAG_DISTANCE ) &&
                 !( Math.Abs( currentPosition.Y - _lastMouseDown.Y ) > MINIMUM_DRAG_DISTANCE ) )
            {
                return;
            }

            Window window = element.GetPresentationSource()?.RootVisual as Window;

            if ( window?.WindowState != WindowState.Maximized )
            {
                return;
            }

            window.WindowState = WindowState.Normal;
        }

        private static void OnMouseDown( object sender, PointerPressedEventArgs e )
        {
            if ( !( sender is Control element ) )
            {
                return;
            }

            if ( e.GetCurrentPoint( element ).Properties.IsLeftButtonPressed )
            {
                _lastMouseDown = e.GetPosition( element );
            }

            Window window = element.GetPresentationSource()?.RootVisual as Window;

            if ( window == null || window.WindowState == WindowState.Maximized )
            {
                return;
            }

            window.BeginMoveDrag( e );
        }
    }
}