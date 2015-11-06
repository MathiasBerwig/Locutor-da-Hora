using System;
using System.Globalization;
using System.Windows.Data;

namespace Locutor_da_Hora.Converters
{
    class VolumeRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float val = (float)value;
            if (val > 0.75)
                return 3;
            if (val > 0.5)
                return 2;
            if (val > 0)
                return 1;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
