using System.Windows;

namespace Locutor_da_Hora.Utils
{
    internal class MessageHelper
    {
        #region Membros Privados
        private static readonly string NomeAplicacao = Properties.Resources.NomeAplicacao;
        #endregion

        #region Métodos Públicos
        public static void ShowError(string message)
        {
            MessageBox.Show(message, NomeAplicacao, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowWarning(string message)
        {
            MessageBox.Show(message, NomeAplicacao, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        #endregion
    }
}