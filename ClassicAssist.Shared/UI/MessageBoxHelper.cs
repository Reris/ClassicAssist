#region License

// Copyright (C) 2026 Reetus
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
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Shared.UI
{
    public enum MessageBoxResult
    {
        None = 0,
        OK,
        Cancel,
        Yes,
        No
    }

    public enum MessageBoxButton
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    public enum MessageBoxImage
    {
        None,
        Error,
        Question,
        Warning,
        Information
    }

    /// <summary>
    ///     Cross-platform replacement for ClassicAssist.Shared.UI.MessageBoxHelper. Fire-and-forget
    ///     overloads mirror the WPF API; result-aware call sites should await
    ///     <see cref="ShowAsync" />.
    /// </summary>
    public static class MessageBoxHelper
    {
        public static void Show( string message )
        {
            _ = ShowAsync( message );
        }

        public static void Show( string message, string title )
        {
            _ = ShowAsync( message, title );
        }

        public static void Show( string message, string title, MessageBoxButton button,
            MessageBoxImage icon = MessageBoxImage.None )
        {
            _ = ShowAsync( message, title, button, icon );
        }

        public static void Show( string message, string title, MessageBoxButton button, MessageBoxImage icon,
            MessageBoxResult defaultResult )
        {
            _ = ShowAsync( message, title, button, icon, defaultResult );
        }

        public static async Task<MessageBoxResult> ShowAsync( string message, string title = null,
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None )
        {
            return await Dispatcher.UIThread.InvokeAsync( () => ShowCore( message, title, button, icon, defaultResult ) );
        }

        private static async Task<MessageBoxResult> ShowCore( string message, string title, MessageBoxButton button,
            MessageBoxImage icon, MessageBoxResult defaultResult )
        {
            if ( string.IsNullOrEmpty( title ) )
            {
                title = "ClassicAssist";
            }

            ButtonEnum buttons = button switch
            {
                MessageBoxButton.OK => ButtonEnum.Ok,
                MessageBoxButton.OKCancel => ButtonEnum.OkCancel,
                MessageBoxButton.YesNo => ButtonEnum.YesNo,
                MessageBoxButton.YesNoCancel => ButtonEnum.YesNoCancel,
                _ => ButtonEnum.Ok
            };

            Icon iconKind = icon switch
            {
                MessageBoxImage.Error => Icon.Error,
                MessageBoxImage.Question => Icon.Question,
                MessageBoxImage.Warning => Icon.Warning,
                MessageBoxImage.Information => Icon.Info,
                _ => Icon.None
            };

            MessageBoxResult fallback = defaultResult == MessageBoxResult.None
                ? button == MessageBoxButton.YesNo
                    ? MessageBoxResult.No
                    : button == MessageBoxButton.OKCancel ? MessageBoxResult.Cancel : MessageBoxResult.OK
                : defaultResult;

            try
            {
                var box = MessageBoxManager.GetMessageBoxStandard( title, message, buttons, iconKind,
                    windowStartupLocation: WindowStartupLocation.CenterScreen );

                Window owner = GetOwner();

                ButtonResult result = owner != null
                    ? await box.ShowWindowDialogAsync( owner )
                    : await box.ShowWindowAsync();

                return result switch
                {
                    ButtonResult.Ok => MessageBoxResult.OK,
                    ButtonResult.Yes => MessageBoxResult.Yes,
                    ButtonResult.No => MessageBoxResult.No,
                    ButtonResult.Cancel => MessageBoxResult.Cancel,
                    _ => fallback
                };
            }
            catch ( Exception )
            {
                return fallback;
            }
        }

        private static Window GetOwner()
        {
            return DialogHelper.GetActiveWindow();
        }
    }
}
