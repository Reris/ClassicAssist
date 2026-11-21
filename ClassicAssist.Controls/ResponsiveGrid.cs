#region License

// Copyright (C) $CURRENT_YEAR$ Reetus
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace ClassicAssist.Controls
{
    public class ResponsiveGrid : Grid
    {
        private bool _loaded;
        private double _minWidth;

        public ResponsiveGrid()
        {
            Loaded += OnLoaded;
        }

        public List<Control> Items { get; set; } = new List<Control>();

        private void OnSizeChanged( object sender, SizeChangedEventArgs e )
        {
            UpdateItems();
        }

        private void OnLoaded( object sender, RoutedEventArgs e )
        {
            _minWidth = GetMinimumWidth();
            UpdateItems();
        }

        private double GetMinimumWidth()
        {
            double minWidth = MinWidth;

            foreach ( Control uiElement in Items )
            {
                uiElement.Measure( new Size( Bounds.Width, Bounds.Height ) );

                if ( uiElement.DesiredSize.Width > MinWidth )
                {
                    minWidth = uiElement.DesiredSize.Width + 10;
                }
            }

            return minWidth;
        }

        private void UpdateItems()
        {
            Children.Clear();
            ColumnDefinitions.Clear();

            UpdateLayout();

            double availableHeight = Bounds.Height;
            double availableWidth = Bounds.Width;

            double currentHeight = 0;

            int columns = 1;
            bool wontFit = false;

            HashSet<Control> items = Items.ToHashSet();

            Dictionary<int, Size> measuredSizes = new Dictionary<int, Size>( items.Count );

            foreach ( Control uiElement in items )
            {
                int hash = RuntimeHelpers.GetHashCode( uiElement );

                if ( !measuredSizes.ContainsKey( hash ) )
                {
                    uiElement.Measure( new Size( availableWidth, availableHeight ) );
                    measuredSizes[hash] = uiElement.DesiredSize;
                }

                if ( currentHeight + uiElement.DesiredSize.Height > availableHeight )
                {
                    if ( availableWidth / ( columns + 1 ) < _minWidth )
                    {
                        wontFit = true;
                        continue;
                    }

                    columns++;
                    currentHeight = 0;
                }

                currentHeight += (int) uiElement.DesiredSize.Height;
            }

            availableHeight = Bounds.Height;

            if ( wontFit )
            {
                double totalHeight = ( from uiElement in items select RuntimeHelpers.GetHashCode( uiElement ) into hash select measuredSizes[hash].Height ).Sum();

                availableHeight = Math.Min( availableHeight, totalHeight / columns );
            }

            for ( int i = 0; i < columns; i++ )
            {
                if ( !items.Any() )
                {
                    break;
                }

                currentHeight = 0;
                StackPanel stackPanel = new StackPanel();
                ColumnDefinitions.Add( new ColumnDefinition() );

                if ( wontFit )
                {
                    ScrollViewer scrollViewer = new ScrollViewer { Content = stackPanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                    SetColumn( scrollViewer, i );
                    Children.Add( scrollViewer );
                }
                else
                {
                    SetColumn( stackPanel, i );
                    Children.Add( stackPanel );
                }

                int row = 0;

                foreach ( Control uiElement in items.ToHashSet() )
                {
                    int hash = RuntimeHelpers.GetHashCode( uiElement );

                    if ( ( !wontFit || i + 1 < columns ) && currentHeight + measuredSizes[hash].Height > availableHeight )
                    {
                        continue;
                    }

                    if ( uiElement is Control frameworkElement )
                    {
                        if ( i + 1 < columns )
                        {
                            frameworkElement.Margin = new Thickness( 0, row != 0 ? 5 : 0, 5, 0 );
                        }
                        else if ( row != 0 )
                        {
                            frameworkElement.Margin = new Thickness( 0, 5, 0, 0 );
                        }
                        else
                        {
                            frameworkElement.Margin = new Thickness( 0, 0, 0, 0 );
                        }
                    }

                    currentHeight += measuredSizes[hash].Height;

                    AvaloniaObject parent = uiElement.GetVisualParent() as AvaloniaObject;

                    if ( parent is Panel panel )
                    {
                        panel.Children.Remove( uiElement );
                    }

                    stackPanel.Children.Add( uiElement );
                    items.Remove( uiElement );
                    row++;
                }
            }

            if ( _loaded )
            {
                return;
            }

            _loaded = true;
            SizeChanged += OnSizeChanged;
        }
    }
}