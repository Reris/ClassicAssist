// Copyright (C) 2024 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.Shared.UI.Compat;
using System.Windows.Input;

namespace ClassicAssist.UI.Views.Agents.Organizer
{
    /// <summary>
    ///     Interaction logic for SourceDestinationOverrideControl.axaml
    /// </summary>
    public partial class SourceDestinationOverrideControl : UserControl
    {
        public static readonly Avalonia.StyledProperty<int?> SerialProperty =
        AvaloniaCompat.RegisterCompat<SourceDestinationOverrideControl, int?>(nameof( Serial ), null);

        public static readonly Avalonia.StyledProperty<ICommand> SetCommandProperty =
        AvaloniaCompat.RegisterCompat<SourceDestinationOverrideControl, ICommand>(nameof( SetCommand ), null);

        public static readonly Avalonia.StyledProperty<object> SetCommandParameterProperty =
        AvaloniaCompat.RegisterCompat<SourceDestinationOverrideControl, object>(nameof( SetCommandParameter ), null);

        public static readonly Avalonia.StyledProperty<ICommand> ClearCommandProperty =
        AvaloniaCompat.RegisterCompat<SourceDestinationOverrideControl, ICommand>(nameof( ClearCommand ), null);

        public SourceDestinationOverrideControl()
        {
            InitializeComponent();
        }

        public ICommand ClearCommand
        {
            get => (ICommand) GetValue( ClearCommandProperty );
            set => SetValue( ClearCommandProperty, value );
        }

        public int? Serial
        {
            get => (int?) GetValue( SerialProperty );
            set => SetValue( SerialProperty, value );
        }

        public ICommand SetCommand
        {
            get => (ICommand) GetValue( SetCommandProperty );
            set => SetValue( SetCommandProperty, value );
        }

        public object SetCommandParameter
        {
            get => GetValue( SetCommandParameterProperty );
            set => SetValue( SetCommandParameterProperty, value );
        }
    }
}