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
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace ClassicAssist.Shared.UI.ValueConverters
{
    /// <summary>
    ///     Returns <c>true</c> when the bound value equals <c>ConverterParameter</c>. Values are
    ///     compared with <see cref="object.Equals" />, falling back to an invariant, case-insensitive
    ///     string comparison so that e.g. an <see cref="int" /> matches a "0" parameter.
    /// </summary>
    public class ValueEqualityConverter : IValueConverter
    {
        public static readonly ValueEqualityConverter Instance = new ValueEqualityConverter();

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( value == null && parameter == null )
            {
                return true;
            }

            if ( value == null || parameter == null )
            {
                return false;
            }

            if ( Equals( value, parameter ) )
            {
                return true;
            }

            string valueString = value as string ?? System.Convert.ToString( value, CultureInfo.InvariantCulture );
            string parameterString = parameter as string ?? System.Convert.ToString( parameter, CultureInfo.InvariantCulture );

            return string.Equals( valueString, parameterString, StringComparison.OrdinalIgnoreCase );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
