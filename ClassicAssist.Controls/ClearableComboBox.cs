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
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input;
using ClassicAssist.Shared.UI.Compat;
using System.Windows.Input;

namespace ClassicAssist.Controls
{
    public class ClearableComboBox : ComboBox
    {
        public static readonly Avalonia.StyledProperty<ICommand> ClearCommandProperty =
        AvaloniaCompat.RegisterCompat<ClearableComboBox, ICommand>(nameof( ClearCommand ), default);

        public static readonly Avalonia.StyledProperty<object> ClearCommandParameterProperty =
        AvaloniaCompat.RegisterCompat<ClearableComboBox, object>(nameof( ClearCommandParameter ), null);

        public ICommand ClearCommand
        {
            get => (ICommand) GetValue( ClearCommandProperty );
            set => SetValue( ClearCommandProperty, value );
        }

        public object ClearCommandParameter
        {
            get => GetValue( ClearCommandParameterProperty );
            set => SetValue( ClearCommandParameterProperty, value );
        }

        protected override void OnApplyTemplate( TemplateAppliedEventArgs e )
        {
            base.OnApplyTemplate( e );

            Button button = e.NameScope.Find<Button>( "ClearButton" );

            if ( button != null )
            {
                // OnApplyTemplate can run more than once; avoid double-wiring the handler.
                button.Click -= ClearButtonOnClick;
                button.Click += ClearButtonOnClick;
            }
        }

        private void ClearButtonOnClick( object sender, RoutedEventArgs e )
        {
            SelectedItem = null;
            ClearCommand?.Execute( ClearCommandParameter );
        }
    }
}