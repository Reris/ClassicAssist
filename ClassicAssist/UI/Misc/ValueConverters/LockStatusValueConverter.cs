using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia;
using Avalonia.Media;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class LockStatusValueConverter : IValueConverter
    {
        private static readonly Lazy<IImage> _upImage =
            new Lazy<IImage>( () => Properties.Resources.arrow_up.ToImageSource() );

        private static readonly Lazy<IImage> _downImage =
            new Lazy<IImage>( () => Properties.Resources.arrow_down.ToImageSource() );

        private static readonly Lazy<IImage> _lockedImage =
            new Lazy<IImage>( () => Properties.Resources.padlock.ToImageSource() );

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( !( value is LockStatus ) )
            {
                return value;
            }

            LockStatus lockStatus = (LockStatus) value;

            switch ( lockStatus )
            {
                case LockStatus.Up:

                    return _upImage.Value;
                case LockStatus.Down:

                    return _downImage.Value;
                case LockStatus.Locked:

                    return _lockedImage.Value;
                default:
                    return _upImage.Value;
                    //throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}