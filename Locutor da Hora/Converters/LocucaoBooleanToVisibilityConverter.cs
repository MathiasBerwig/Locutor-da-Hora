using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using Locutor_da_Hora.Model;

namespace Locutor_da_Hora.Converters
{    
    /// <summary>
    /// Conversor de valores booleanos e de Locução para Visibility.
    /// Utilizado para mostrar o "BtExcluir" de cada Locução dentro da página ListaLocucoes.xaml.
    /// Retorna Visibility.Visible caso todos os booleanos informados sejam True e a primeira Locucao.UniqueName seja diferente de "adicionar_locucao". Caso contrário, Visibility.Hidden.
    /// </summary>
    class LocucaoBooleanToVisibilityConverter : IMultiValueConverter
    {       
        public object Convert(object[] values,
                            Type targetType,
                            object parameter,
                            System.Globalization.CultureInfo culture)
        {
            bool visible = values.OfType<bool>().Aggregate(true, (current, value) => current && value);
            Locucao locucao = values.OfType<Locucao>().First();

            return visible && !locucao.UniqueName.Equals("adicionar_locucao") && (!locucao.ReadOnly || App.DebugMode) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        public object[] ConvertBack(object value,
                                    Type[] targetTypes,
                                    object parameter,
                                    System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
