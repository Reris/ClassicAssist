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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Threading;
using ClassicAssist.Shared.UI;
using ClassicAssist.Shared.UI.Compat;
using System.Windows.Input;

namespace ClassicAssist.Controls
{
    /// <summary>
    ///     Interaction logic for MultiValueSelector.axaml
    /// </summary>
    public partial class MultiValueSelector : UserControl, INotifyPropertyChanged
    {
        public static readonly Avalonia.StyledProperty<ObservableCollection<int>> ValuesProperty =
        AvaloniaCompat.RegisterCompat<MultiValueSelector, ObservableCollection<int>>(nameof( Values ), null, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, onChanged: OnValuesChanged);

        public static readonly Avalonia.StyledProperty<int> PopupWidthProperty =
        AvaloniaCompat.RegisterCompat<MultiValueSelector, int>(nameof( PopupWidth ), 200, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly Avalonia.StyledProperty<object> ButtonsProperty =
        AvaloniaCompat.RegisterCompat<MultiValueSelector, object>(nameof( Buttons ), default);

        public static readonly Avalonia.StyledProperty<bool> HexDisplayProperty =
        AvaloniaCompat.RegisterCompat<MultiValueSelector, bool>(nameof( HexDisplay ), default);

        public static readonly Avalonia.StyledProperty<DataTemplate> ItemTemplateProperty =
        AvaloniaCompat.RegisterCompat<MultiValueSelector, DataTemplate>(nameof( ItemTemplate ), default);

        private RelayCommand _removeItemCommand;

        public MultiValueSelector()
        {
            InitializeComponent();
        }

        public object Buttons
        {
            get => GetValue( ButtonsProperty );
            set => SetValue( ButtonsProperty, value );
        }

        public bool HexDisplay
        {
            get => (bool) GetValue( HexDisplayProperty );
            set => SetValue( HexDisplayProperty, value );
        }

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) GetValue( ItemTemplateProperty );
            set => SetValue( ItemTemplateProperty, value );
        }

        public int PopupWidth
        {
            get => (int) GetValue( PopupWidthProperty );
            set => SetValue( PopupWidthProperty, value );
        }

        public ICommand RemoveItemCommand =>
            _removeItemCommand ?? ( _removeItemCommand = new RelayCommand( v =>
            {
                if ( !( v is int value ) )
                {
                    return;
                }

                Values.Remove( value );
            } ) );

        public ObservableCollection<int> Values
        {
            get => (ObservableCollection<int>) GetValue( ValuesProperty );
            set => SetValue( ValuesProperty, value );
        }

        public string ValuesDisplay => string.Join( ", ", Values?.Select( v => HexDisplay ? $"0x{v:x}" : $"{v}" ) ?? Array.Empty<string>() );

        public new event PropertyChangedEventHandler PropertyChanged;

        public void OnValuesCollectionChanged( object s, NotifyCollectionChangedEventArgs args )
        {
            OnPropertyChanged( nameof( ValuesDisplay ) );
        }

        private static void OnValuesChanged( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
            if ( e.NewValue == null )
            {
                return;
            }

            if ( !( d is MultiValueSelector selector ) )
            {
                return;
            }

            selector.OnPropertyChanged( nameof( ValuesDisplay ) );

            if ( e.OldValue is ObservableCollection<int> oldCollection )
            {
                oldCollection.CollectionChanged -= selector.OnValuesCollectionChanged;
            }

            if ( e.NewValue is ObservableCollection<int> newCollection )
            {
                newCollection.CollectionChanged += selector.OnValuesCollectionChanged;
            }
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            Popup.IsOpen = !Popup.IsOpen;
        }

        private void TextBox_PreviewKeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key != Key.Enter )
            {
                return;
            }

            if ( Values == null )
            {
                Values = new ObservableCollection<int>();
            }

            string text = ( (TextBox) sender ).Text;

            if ( text.StartsWith( "0x" ) )
            {
                if ( int.TryParse( text.Substring( 2 ), NumberStyles.HexNumber, null, out int value ) && !Values.Contains( value ) )
                {
                    Values.Add( value );
                }
            }
            else
            {
                if ( int.TryParse( text, out int value ) && !Values.Contains( value ) )
                {
                    Values.Add( value );
                }
            }

            ( (TextBox) sender ).Clear();

            e.Handled = true;
        }

        private void Popup_Opened( object sender, EventArgs e )
        {
            Dispatcher.UIThread.Post( () =>
            {
                TextBox.Focus();
            }, DispatcherPriority.Input );
        }

        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}