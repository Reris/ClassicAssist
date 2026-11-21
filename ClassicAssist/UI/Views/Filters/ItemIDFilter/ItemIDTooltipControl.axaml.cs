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

using System.Collections.Generic;
using System.ComponentModel;

using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;
using ClassicAssist.Shared.UI.Compat;
using Avalonia.Media.Imaging;

namespace ClassicAssist.UI.Views.Filters.ItemIDFilter
{
    /// <summary>
    ///     Interaction logic for ItemIDTooltipControl.axaml
    /// </summary>
    public partial class ItemIDTooltipControl : UserControl, INotifyPropertyChanged
    {
        public static readonly Avalonia.StyledProperty<int> ItemIDProperty =
        AvaloniaCompat.RegisterCompat<ItemIDTooltipControl, int>(nameof( ItemID ), 0, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, onChanged: OnItemIDChanged);

        public static readonly Avalonia.StyledProperty<int> HueProperty =
        AvaloniaCompat.RegisterCompat<ItemIDTooltipControl, int>(nameof( Hue ), 0, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, onChanged: OnHueChanged);

        private IImage _image;
        private string _itemName;

        public ItemIDTooltipControl()
        {
            InitializeComponent();
        }

        public int Hue
        {
            get => (int) GetValue( HueProperty );
            set => SetValue( HueProperty, value );
        }

        public IImage Image
        {
            get => _image;
            set => SetField( ref _image, value );
        }

        public int ItemID
        {
            get => (int) GetValue( ItemIDProperty );
            set => SetValue( ItemIDProperty, value );
        }

        public string ItemName
        {
            get => _itemName;
            set => SetField( ref _itemName, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnHueChanged( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
            if ( !( d is ItemIDTooltipControl control ) )
            {
                return;
            }

            if ( e.NewValue != e.OldValue )
            {
                control.UpdateImage();
            }
        }

        private static void OnItemIDChanged( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
            if ( !( d is ItemIDTooltipControl control ) )
            {
                return;
            }

            if ( e.NewValue != e.OldValue )
            {
                control.UpdateImage();
            }
        }

        private void UpdateImage()
        {
            Bitmap bitmap = Art.GetStatic( ItemID, Hue > 0 ? Hue : 0 );
            Image = bitmap;
            ItemName = TileData.GetStaticTile( ItemID ).Name;
        }

        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected bool SetField<T>( ref T field, T value, [CallerMemberName] string propertyName = null )
        {
            if ( EqualityComparer<T>.Default.Equals( field, value ) )
            {
                return false;
            }

            field = value;
            OnPropertyChanged( propertyName );
            return true;
        }
    }
}