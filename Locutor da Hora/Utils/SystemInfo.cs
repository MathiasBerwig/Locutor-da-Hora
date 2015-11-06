using System;
using System.Globalization;
using Locutor_da_Hora.Properties;

namespace Locutor_da_Hora.Utils
{
    /// <summary>
    /// Classe para reunir informações do sistema.
    /// </summary>
    static class SystemInfo
    {
        #region Membros Públicos
        public static double PrimaryScreenWidth => System.Windows.SystemParameters.PrimaryScreenWidth;
        public static double PrimaryScreenHeight => System.Windows.SystemParameters.PrimaryScreenHeight;
        public static string CurrentCulture => CultureInfo.InstalledUICulture.Name;
        public static string ScreenBitDepth => System.Windows.Forms.Screen.PrimaryScreen.BitsPerPixel + "-bits";             

        public static string UniqueID
        {
            get
            {
                var clientId = Settings.Default.ClientId;
                if (String.IsNullOrWhiteSpace(clientId))
                {
                    clientId = Guid.NewGuid().ToString();
                    Settings.Default.ClientId = clientId;
                    Settings.Default.Save();
                }

                return clientId;
            }            
        }
        #endregion
    }
}
