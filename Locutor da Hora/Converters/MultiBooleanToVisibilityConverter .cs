using System;
using System.Linq;
using System.Windows.Data;

namespace Locutor_da_Hora.Converters
{
    /// <summary>
    /// Conversor de múltiplos valores booleanos para Visibility.
    /// Para o tipo AND: retorna Visibility.Visible se todos os valores forem True; Caso contrário, Visibility.Hidden.
    /// Para o tipo OR: retorna Visibility.Visible se algum dos valores informados for True; Caso contrário, Visibility.Hidden.
    /// </summary>
    class MultiBooleanToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values,
                            Type targetType,
                            object parameter,
                            System.Globalization.CultureInfo culture)
        {
            ConversionType conversionType = values.OfType<ConversionType>().First();

            bool visible = conversionType == ConversionType.AND
                ? values.OfType<bool>().All(x => x.Equals(true))
                : values.OfType<bool>().Any(x => x.Equals(true));

            return visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        public object[] ConvertBack(object value,
                                    Type[] targetTypes,
                                    object parameter,
                                    System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum ConversionType
    {
        AND, OR
    }
}
