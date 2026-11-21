#region License

// Copyright (C) 2025 Reetus
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace ClassicAssist.UI.Misc.Behaviours
{
    /// <summary>
    ///     Under WPF this behaviour hooked WM_GETMINMAXINFO so a borderless window maximized to the
    ///     monitor work area instead of covering the taskbar. Avalonia's windowing platform
    ///     implementation already clamps maximized borderless windows to the work area, so the
    ///     behaviour is retained only for XAML compatibility and does nothing.
    /// </summary>
    public class WindowMaximizeBehaviour : Behavior<Window>
    {
    }
}