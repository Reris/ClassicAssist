#region License

// Copyright (C) 2021 Reetus
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
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Shared.UI.Behaviours
{
    public class GridSizeChangedBehaviour : Behavior<Grid>
    {
        public static readonly Avalonia.StyledProperty<double> WidthProperty =
        AvaloniaCompat.RegisterCompat<GridSizeChangedBehaviour, double>(nameof( Width ), 0.0, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, onChanged: WidthChangedCallback);

        public static readonly Avalonia.StyledProperty<double> HeightProperty =
        AvaloniaCompat.RegisterCompat<GridSizeChangedBehaviour, double>(nameof( Height ), 0.0, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, onChanged: HeightChangedCallback);

        public double Height
        {
            get => (double) GetValue( HeightProperty );
            set => SetValue( HeightProperty, value );
        }

        public double Width
        {
            get => (double) GetValue( WidthProperty );
            set => SetValue( WidthProperty, value );
        }

        private static void WidthChangedCallback( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
        }

        private static void HeightChangedCallback( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SizeChanged += OnSizeChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SizeChanged -= OnSizeChanged;
        }

        private void OnSizeChanged( object sender, SizeChangedEventArgs e )
        {
            if ( !( sender is Grid grid ) )
            {
                return;
            }

            if ( e.WidthChanged && e.NewSize.Width > 0 )
            {
                if ( Math.Abs( Width - e.NewSize.Width ) > 0 )
                {
                    Width = e.NewSize.Width + grid.Margin.Left + grid.Margin.Right;
                }
            }

            if ( e.HeightChanged && e.NewSize.Height > 0 )
            {
                Height = e.NewSize.Height + grid.Margin.Top + grid.Margin.Bottom;
            }
        }
    }
}