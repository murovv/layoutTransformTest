using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace LayoutTransformTest.Converter;

public class ZoomBorderSizeConverter:IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Control c)
        {
            #warning во избежании уменьшения размера во время зума делаем бордер огромным
            return Math.Max(c.Width, c.Height) * 10;
        }

        return 10000;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}