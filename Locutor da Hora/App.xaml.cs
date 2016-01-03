using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Locutor_da_Hora.Pages;
using Locutor_da_Hora.Utils;
using System.IO;

namespace Locutor_da_Hora
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region Membros Públicos
        public static bool DebugMode;
        public List<string> CommandLineArguments { get; set; }
        #endregion

        #region Métodos
        void App_Startup(object sender, StartupEventArgs e)
        {
            // Recebe os parâmetros de execução do aplicativo
            CommandLineArguments = Environment.GetCommandLineArgs().ToList();

            // Define o modo de debug            
            DebugMode = CommandLineArguments.Any(s => s.Equals("debug", StringComparison.OrdinalIgnoreCase));

            GoogleAnalyticsTracker.Instance.TrackEvent(Contract.Analytics.APLICACAO, Contract.Analytics.Aplicacao.INICIALIZACAO, null, null);

            // Verifica se o computador atende os requisitos mínimos da aplicação. Mensagens de erro ativadas.
            if (!VerificarRequisitos(true)) Current.Shutdown();            

            // Cria a pasta %Locutor da Hora%\Gravações
            CriarPastaTemporaria();

            // Verifica se existem atualizações disponíveis
            UpdateHelper.Instance.CheckLatestVersion();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            GoogleAnalyticsTracker.Instance.TrackEvent(Contract.Analytics.APLICACAO, Contract.Analytics.Aplicacao.FINALIZACAO, null, null);

            // Exclui todos os arquivos da pasta temporária
            LimparPastaTemporaria();
        }

        /// <summary>
        /// Verifica se o computador atende os requisitos mínimos da aplicação. <br/>        
        /// </summary>
        /// <param name="mostrarErros"><code>true</code> para mostrar um MessageBox com os erros.</param>
        /// <returns><code>true</code> caso o computador atenda todos os requisitos obrigatórios, <code>false</code> caso contrário.</returns>
        private bool VerificarRequisitos(bool mostrarErros)
        {
            var erros = new ArrayList();
            var errosGraves = new ArrayList();

            // Verifica a resolução do monitor primário
            if (SystemInfo.PrimaryScreenHeight < 768 || SystemInfo.PrimaryScreenWidth < 1024)
                errosGraves.Add(Locutor_da_Hora.Properties.Resources.Exception_ResolucaoNaoSuportada);

            // Verifica dispositivo de gravação
            if (Audio.Captura.Instance.AtualizarDispositivosGravacao() < 1)
            {
                erros.Add(Locutor_da_Hora.Properties.Resources.Exception_SemMicrofone);
                Gravacao.Instance.PodeGravar = false;
            }
                
            foreach (string erro in erros)
            {
                GoogleAnalyticsTracker.Instance.TrackException(erro, false);
                if (mostrarErros) MessageHelper.ShowError(erro);
            }

            foreach (string erro in errosGraves)
            {
                GoogleAnalyticsTracker.Instance.TrackException(erro, true);
                if (mostrarErros) MessageHelper.ShowError(erro);
            }

            return errosGraves.Count == 0;
        }

        private void CriarPastaTemporaria()
        {
            try
            {
                System.IO.Directory.CreateDirectory(Environment.CurrentDirectory + Locutor_da_Hora.Properties.Resources.PastaTemporaria_MP3);
            }
            catch (Exception exception)
            {
                GoogleAnalyticsTracker.Instance.TrackException(exception.Message, true);
            }
        }

        private void LimparPastaTemporaria()
        {
            try
            {
                System.IO.DirectoryInfo pastaTemporaria = new DirectoryInfo(Environment.CurrentDirectory + Locutor_da_Hora.Properties.Resources.PastaTemporaria_MP3);

                foreach (FileInfo file in pastaTemporaria.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo dir in pastaTemporaria.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception exception)
            {
                GoogleAnalyticsTracker.Instance.TrackException(exception.Message, true);
            }
        }
        #endregion
    }
}
