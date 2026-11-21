#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Threading;
using Assistant;
using Avalonia.Xaml.Interactivity;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class SmartOnTopBehaviour : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            Engine.FocusChangedEvent += OnFocusChangedEvent;
        }

        private void OnFocusChangedEvent( bool focus )
        {
            Dispatcher.UIThread.Post( () => { AssociatedObject.Topmost = focus; }, DispatcherPriority.Background );
        }

        protected override void OnDetaching()
        {
            Engine.FocusChangedEvent -= OnFocusChangedEvent;

            base.OnDetaching();
        }
    }
}