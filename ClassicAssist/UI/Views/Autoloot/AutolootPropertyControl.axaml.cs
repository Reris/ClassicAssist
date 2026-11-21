// Copyright (C) 2023 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.UI.ViewModels.Agents;

namespace ClassicAssist.UI.Views.Autoloot
{
    /// <summary>
    ///     Interaction logic for AutolootPropertyControl.axaml
    /// </summary>
    public partial class AutolootPropertyControl : UserControl
    {
        public AutolootPropertyControl()
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

            if ( autolootConstraintEntry.Property.AllowedValuesEnum != null &&
                 autolootConstraintEntry.Property.AllowedValuesEnum == typeof( SkillBonusSkills ) )
            {
                Binding selectedItemBinding = new Binding( nameof( autolootConstraintEntry.Additional ) )
                {
                    Source = autolootConstraintEntry, Mode = BindingMode.TwoWay
                };

                List<string> source = ( from object value in typeof( SkillBonusSkills ).GetEnumValues()
                    let fieldInfo = typeof( SkillBonusSkills ).GetField( value.ToString() )
                    let attr = (DescriptionAttribute) fieldInfo
                        .GetCustomAttributes( typeof( DescriptionAttribute ), false ).FirstOrDefault()
                    select attr == null ? value.ToString() : attr.Description ).ToList();

                ComboBox comboBox = new ComboBox { Width = 140, ItemsSource = source };

                comboBox.Bind( SelectingItemsControl.SelectedItemProperty, selectedItemBinding );

                Content = comboBox;
            }
            else
            {
                Content = new TextBlock { Text = autolootConstraintEntry.Property.Name };
            }
        }
    }
}