// Copyright (C) 2023 Reetus
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
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Controls
{
    /// <summary>
    ///     A CheckBox that hosts an additional <see cref="ChildContent" /> control ( shown to the right of
    ///     the label ) which is only enabled while the box is checked.
    /// </summary>
    public class OptionedCheckBoxControl : CheckBox
    {
        public static readonly Avalonia.StyledProperty<object> ChildContentProperty =
        AvaloniaCompat.RegisterCompat<OptionedCheckBoxControl, object>(nameof( ChildContent ), null, onChanged: OnChildContentChanged);

        private object _label;
        private bool _rebuilding;

        public object ChildContent
        {
            get => GetValue( ChildContentProperty );
            set => SetValue( ChildContentProperty, value );
        }

        protected override void OnPropertyChanged( AvaloniaPropertyChangedEventArgs e )
        {
            base.OnPropertyChanged( e );

            if ( e.Property == ContentProperty )
            {
                OnContentChanged( e.OldValue, e.NewValue );
            }
            else if ( e.Property == IsCheckedProperty )
            {
                UpdateChildEnabled();
            }
        }

        private void OnContentChanged( object oldContent, object newContent )
        {
            // Ignore the composed Content we assign ourselves while rebuilding the layout, otherwise
            // we would capture the DockPanel as the label and nest it on the next rebuild.
            if ( _rebuilding )
            {
                return;
            }

            _label = newContent;
            Rebuild();
        }

        private static void OnChildContentChanged( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
            ( (OptionedCheckBoxControl) d ).Rebuild();
        }

        private void Rebuild()
        {
            if ( ChildContent == null )
            {
                // No extra content - behave as a normal checkbox showing its label.
                if ( Content is DockPanel )
                {
                    SetContentInternal( _label );
                }

                return;
            }

            DockPanel dockPanel = new DockPanel { LastChildFill = true };

            ContentPresenter checkBoxPresenter = new ContentPresenter
            {
                Content = _label, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            DockPanel.SetDock( checkBoxPresenter, Dock.Left );
            dockPanel.Children.Add( checkBoxPresenter );

            ContentPresenter childContentPresenter = new ContentPresenter
            {
                Content = ChildContent, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };
            DockPanel.SetDock( childContentPresenter, Dock.Right );
            dockPanel.Children.Add( childContentPresenter );

            SetContentInternal( dockPanel );

            UpdateChildEnabled();
        }

        private void SetContentInternal( object content )
        {
            _rebuilding = true;
            Content = content;
            _rebuilding = false;
        }

        private void UpdateChildEnabled()
        {
            if ( ChildContent is Control childElement )
            {
                childElement.IsEnabled = IsChecked ?? false;
            }
        }
    }
}
