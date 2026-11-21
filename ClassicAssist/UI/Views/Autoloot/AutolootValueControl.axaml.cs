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

using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ClassicAssist.Controls;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI.ValueConverters;
using ClassicAssist.UI.Misc;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Views.Autoloot
{
    /// <summary>
    ///     Interaction logic for AutolootValueControl.axaml
    /// </summary>
    public partial class AutolootValueControl : UserControl
    {
        public AutolootValueControl()
        {
            InitializeComponent();
        }

        private void OnDataContextChanged( object sender, AvaloniaPropertyChangedEventArgs e )
        {
            // TODO: Convert to XAML
            if ( !( e.NewValue is AutolootConstraintEntry autolootConstraintEntry ) )
            {
                return;
            }

            Grid.Children.Clear();

            Binding widthBinding = new Binding( "Bounds.Width" ) { RelativeSource = new RelativeSource( RelativeSourceMode.FindAncestor ) { AncestorType = typeof( AutolootValueControl ) } };
            Binding valuesBinding = new Binding( nameof( autolootConstraintEntry.Values ) ) { Source = autolootConstraintEntry, Mode = BindingMode.TwoWay };

            if ( autolootConstraintEntry.Property.AllowedValuesEnum != null && autolootConstraintEntry.Property.AllowedValuesEnum == typeof( Layer ) )
            {
                EnumToIntegerValueConverter enumToIntegerValueConverter = new EnumToIntegerValueConverter();

                Binding selectedItemBinding = new Binding( nameof( autolootConstraintEntry.Value ) )
                {
                    Source = autolootConstraintEntry, Converter = enumToIntegerValueConverter, ConverterParameter = typeof( Layer ), Mode = BindingMode.TwoWay
                };

                EnumBindingSourceExtension itemSource = new EnumBindingSourceExtension( typeof( Layer ) );

                ComboBox comboBox = new ComboBox { ItemsSource = itemSource.ProvideValue( null ) as IEnumerable };

                comboBox.Bind( SelectingItemsControl.SelectedItemProperty, selectedItemBinding );
                comboBox.Bind( WidthProperty, widthBinding );

                Grid.Children.Add( comboBox );

                return;
            }

            if ( autolootConstraintEntry.Property.UseMultipleValues && autolootConstraintEntry.Property.Name != Strings.Cliloc__Multiple_ )
            {
                MultiItemIDSelector control = new MultiItemIDSelector { MinWidth = 40 };
                control.Bind( MultiItemIDSelector.ValuesProperty, valuesBinding );
                control.Bind( WidthProperty, widthBinding );

                Grid.Children.Add( control );

                return;
            }

            if ( autolootConstraintEntry.Property.Name == Strings.Cliloc__Multiple_ )
            {
                MultiClilocSelector control = new MultiClilocSelector { MinWidth = 40 };
                control.Bind( MultiClilocSelector.ValuesProperty, valuesBinding );
                control.Bind( WidthProperty, widthBinding );

                Grid.Children.Add( control );

                return;
            }

            if ( autolootConstraintEntry.Property.Name == Strings.Autoloot_Match )
            {
                AutolootManager manager = AutolootManager.GetInstance();

                Binding selectedItemBinding = new Binding( nameof( autolootConstraintEntry.Additional ) ) { Source = autolootConstraintEntry, Mode = BindingMode.TwoWay };

                ComboBox comboBox = new ComboBox { ItemsSource = manager.GetEntries().Select( ale => ale.Name ) };

                comboBox.Bind( SelectingItemsControl.SelectedItemProperty, selectedItemBinding );
                comboBox.Bind( WidthProperty, widthBinding );

                Grid.Children.Add( comboBox );

                return;
            }

            Binding binding = new Binding( "Value" ) { Source = autolootConstraintEntry };

            EditTextBlock editTextBlock = new EditTextBlock { ShowIcon = true };

            editTextBlock.Bind( EditTextBlock.TextProperty, binding );
            editTextBlock.Bind( WidthProperty, widthBinding );

            Grid.Children.Add( editTextBlock );
        }
    }
}