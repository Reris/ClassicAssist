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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Controls
{
    public class ImageButton : Button
    {
        public static readonly Avalonia.StyledProperty<IImage> ImageSourceProperty =
        AvaloniaCompat.RegisterCompat<ImageButton, IImage>(nameof( ImageSource ), null);

        public static readonly Avalonia.StyledProperty<double> ImageHeightProperty =
        AvaloniaCompat.RegisterCompat<ImageButton, double>(nameof( ImageHeight ), (double) 16);

        public ImageButton()
        {
            Image imageSource = new Image
            {
                Stretch = Stretch.Uniform,
            };

            imageSource[!Image.SourceProperty] = this[!ImageSourceProperty];
            imageSource[!Layoutable.HeightProperty] = this[!ImageHeightProperty];

            Content = imageSource;
        }

        public double ImageHeight
        {
            get => (double) GetValue( ImageHeightProperty );
            set => SetValue( ImageHeightProperty, value );
        }

        public IImage ImageSource
        {
            get => (IImage) GetValue( ImageSourceProperty );
            set => SetValue( ImageSourceProperty, value );
        }
    }
}