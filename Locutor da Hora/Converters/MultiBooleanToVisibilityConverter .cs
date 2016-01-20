using System;
using System.Linq;
using System.Windows.Data;
using static System.Windows.Visibility;

namespace Locutor_da_Hora.Converters
{
    /// <summary>
    /// Conversor de múltiplos valores booleanos para Visibility.
    /// Para o tipo AND: retorna Visible se todos os valores forem True; Caso contrário, Hidden.
    /// Para o tipo AND_DEFAULT_COLLAPSED: retorna Visible se todos os valores forem True; Caso contrário, Collapsed.
    /// Para o tipo OR: retorna Visible se algum dos valores informados for True; Caso contrário, Hidden.
    /// </summary>
    class MultiBooleanToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values,
                            Type targetType,
                            object parameter,
                            System.Globalization.CultureInfo culture)
        {
            ConversionType conversionType = values.OfType<ConversionType>().First();

            switch (conversionType)
            {
                case ConversionType.AND:
                    return values.OfType<bool>().All(x => x.Equals(true)) ? Visible : Hidden;
                case ConversionType.AND_DEFAULT_COLLAPSED:
                    return values.OfType<bool>().All(x => x.Equals(true)) ? Visible : Collapsed;
                case ConversionType.OR:
                    return values.OfType<bool>().Any(x => x.Equals(true)) ? Visible : Hidden;
                default:
                    return Hidden;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum ConversionType
    {
        AND,
        OR,
        AND_DEFAULT_COLLAPSED
    }
}
