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
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using System.Xml;
using Assistant;
using ClassicAssist.Data.Macros;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.Search;
using ClassicAssist.Shared.UI.Compat;
using System.Windows.Input;

namespace ClassicAssist.UI.Views.Macros
{
    /// <summary>
    ///     Interaction logic for MacrosCodeTextEditor.axaml
    /// </summary>
    public partial class MacrosCodeTextEditor : UserControl
    {
        public static readonly Avalonia.StyledProperty<string> FormatErrorProperty =
        AvaloniaCompat.RegisterCompat<MacrosCodeTextEditor, string>(nameof( FormatError ), default( string ));

        public static readonly Avalonia.StyledProperty<bool> IsPerformingActionProperty =
        AvaloniaCompat.RegisterCompat<MacrosCodeTextEditor, bool>(nameof( IsPerformingAction ), false);

        public static readonly Avalonia.StyledProperty<ICommand> ClearExceptionCommandProperty =
        AvaloniaCompat.RegisterCompat<MacrosCodeTextEditor, ICommand>(nameof( ClearExceptionCommand ), default( ICommand ));

        public static readonly Avalonia.StyledProperty<int> CaretPositionProperty =
        AvaloniaCompat.RegisterCompat<MacrosCodeTextEditor, int>(nameof( CaretPosition ), 0);

        public static readonly Avalonia.StyledProperty<TextDocument> DocumentProperty =
        AvaloniaCompat.RegisterCompat<MacrosCodeTextEditor, TextDocument>(nameof( Document ), default( TextDocument ));

        public static readonly Avalonia.StyledProperty<MacroEntry> SelectedItemProperty =
        AvaloniaCompat.RegisterCompat<MacrosCodeTextEditor, MacroEntry>(nameof( SelectedItem ), null);

        public MacrosCodeTextEditor()
        {
            InitializeComponent();
        }

        public int CaretPosition
        {
            get => (int) GetValue( CaretPositionProperty );
            set => SetValue( CaretPositionProperty, value );
        }

        public ICommand ClearExceptionCommand
        {
            get => (ICommand) GetValue( ClearExceptionCommandProperty );
            set => SetValue( ClearExceptionCommandProperty, value );
        }

        public TextDocument Document
        {
            get => (TextDocument) GetValue( DocumentProperty );
            set => SetValue( DocumentProperty, value );
        }

        public string FormatError
        {
            get => (string) GetValue( FormatErrorProperty );
            set => SetValue( FormatErrorProperty, value );
        }

        public bool IsPerformingAction
        {
            get => (bool) GetValue( IsPerformingActionProperty );
            set => SetValue( IsPerformingActionProperty, value );
        }

        public MacroEntry SelectedItem
        {
            get => (MacroEntry) GetValue( SelectedItemProperty );
            set => SetValue( SelectedItemProperty, value );
        }

        private void Grid_Initialized( object sender, EventArgs e )
        {
            CodeTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader( Path.Combine( Engine.StartupPath, "Python.Dark.xshd" ) ), HighlightingManager.Instance );

            SearchPanel.Install( CodeTextEditor );
        }
    }
}