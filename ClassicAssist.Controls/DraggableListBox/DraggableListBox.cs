#region License

// Copyright (C) 2023 Reetus
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion
using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace ClassicAssist.Controls.DraggableListBox
{
    public class DraggableListBox : ListBox
    {
        private object _dragItem;

        private Point _dragStartPosition;

        private bool _pointerDown;

        public DraggableListBox()
        {
            DragDrop.SetAllowDrop( this, true );

            AddHandler( DragDrop.DragOverEvent, OnDragOver );
            AddHandler( DragDrop.DropEvent, OnDrop );
            AddHandler( PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel );
            AddHandler( PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel );
            AddHandler( PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel );
        }

        private void OnPointerPressed( object sender, PointerPressedEventArgs e )
        {
            if ( !e.GetCurrentPoint( this ).Properties.IsLeftButtonPressed )
            {
                return;
            }

            ListBoxItem container = FindAncestor<ListBoxItem>( e.Source as Visual );

            if ( container == null )
            {
                return;
            }

            _dragItem = container.DataContext;
            _dragStartPosition = e.GetPosition( this );
            _pointerDown = true;
        }

        private async void OnPointerMoved( object sender, PointerEventArgs e )
        {
            if ( !_pointerDown || _dragItem == null )
            {
                return;
            }

            Point position = e.GetPosition( this );

            if ( Math.Abs( position.X - _dragStartPosition.X ) < 4 && Math.Abs( position.Y - _dragStartPosition.Y ) < 4 )
            {
                return;
            }

            _pointerDown = false;

            DataObject data = new DataObject();
            data.Set( DataFormats.Text, string.Empty );

#pragma warning disable CS0618
            await DragDrop.DoDragDrop( e, data, DragDropEffects.Move );
#pragma warning restore CS0618
        }

        private void OnPointerReleased( object sender, PointerReleasedEventArgs e )
        {
            _pointerDown = false;
        }

        private void OnDragOver( object sender, DragEventArgs e )
        {
            e.DragEffects = DragDropEffects.Move;
        }

        private void OnDrop( object sender, DragEventArgs e )
        {
            object sourceItem = _dragItem;

            if ( sourceItem == null || !( ItemsSource is IList list ) )
            {
                return;
            }

            ListBoxItem targetContainer = this.GetVisualDescendants().OfType<ListBoxItem>()
                .FirstOrDefault( item => item.Bounds.Contains( e.GetPosition( item ) ) );

            object targetItem = targetContainer?.DataContext;

            int sourceIndex = list.IndexOf( sourceItem );

            if ( sourceIndex < 0 )
            {
                return;
            }

            int targetIndex = targetItem != null ? list.IndexOf( targetItem ) : list.Count - 1;

            if ( targetIndex < 0 )
            {
                targetIndex = list.Count - 1;
            }

            if ( sourceIndex == targetIndex )
            {
                return;
            }

            try
            {
                ( (dynamic) ItemsSource ).Move( sourceIndex, targetIndex );
            }
            catch ( Exception )
            {
                // ItemsSource does not support reordering
            }
        }

        private static T FindAncestor<T>( Visual visual ) where T : Visual
        {
            while ( visual != null && !( visual is T ) )
            {
                visual = visual.GetVisualParent();
            }

            return visual as T;
        }
    }
}