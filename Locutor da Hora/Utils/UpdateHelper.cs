using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Application = System.Windows.Application;

namespace Locutor_da_Hora.Utils
{
    /// <summary>
    /// Classe responsável por realizar a checagem de novas atualizações e instalar as atualizações.
    /// </summary>
    public class UpdateHelper
    {
        #region Membros Privados
        private WebClient webClient = new WebClient();
        private VersionInfo latestVersion;
        private readonly Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        private readonly string downloadedFileDir = AppDomain.CurrentDomain.BaseDirectory;
        private string downloadedFileName;
        private static UpdateHelper instance;
        #endregion

        #region Constantes        
        private const string appID = "br.edu.unijui.locutordahora";
        private const string updateURL = "http://locutordahora.unijui.edu.br/app/update";
        #endregion

        #region Construtores Privados
        private UpdateHelper() { }
        #endregion

        #region Membros Públicos
        public static UpdateHelper Instance => instance ?? (instance = new UpdateHelper());

        public bool UpdateAvailable => latestVersion != null && latestVersion.appID.Equals(appID, StringComparison.CurrentCultureIgnoreCase) && version.CompareTo(new Version(latestVersion.version)) < 0;
        #endregion

        #region Métodos Privados
        private void DownloadUpdate(string address, string filename)
        {
            if (webClient == null) webClient = new WebClient();
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            webClient.DownloadFileAsync(new Uri(address), filename);
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // Libera o WebClient da memória
            ((WebClient)sender).Dispose();

            Properties.Settings.Default.UpdateDownloaded = true;
            Properties.Settings.Default.Save();

            // Notifica o usuário na tela de Boas Vindas
            Pages.BoasVindas.Instance.AtualizacaoDisponivel = true;
        }

        private void WebClient_OnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs downloadStringCompletedEventArgs)
        {
            // Pega a referência do resultado            
            var result = downloadStringCompletedEventArgs.Result;
            // Deserializa o XML recebido do site
            latestVersion = SerializationHelper.ReadFromXml<VersionInfo>(result);
            // Libera o WebClient da memória
            ((WebClient)sender).Dispose();
            
            // Atualiza as configurações, informando a data da última checagem de atualização
            Properties.Settings.Default.LastTimeCheckedUpdate = DateTime.Now;
            Properties.Settings.Default.Save();

            // Atualiza as configurações
            Properties.Settings.Default.UpdateAvailable = UpdateAvailable;
            Properties.Settings.Default.Save();

            // Atualização disponível
            if (!UpdateAvailable) return;            

            // Define o local para salvar o arquivo
            downloadedFileName = downloadedFileDir + System.IO.Path.GetFileName(latestVersion.downloadURL);
            // Baixa atualização
            DownloadUpdate(latestVersion.downloadURL, downloadedFileName);
        }

        private void RunInstaller(string path, string args)
        {
            // Atualiza as configurações, informando que o app foi atualizado
            Properties.Settings.Default.UpdateAvailable = false;
            Properties.Settings.Default.Save();

            // Configura o processo usando as propriedades StartInfo.
            Process process = new Process { StartInfo = { FileName = path, Arguments = args } };
            process.Start();
            Application.Current.Shutdown();
        }
        #endregion

        #region Métodos Públicos
        public void CheckLatestVersion()
        {
            if (webClient == null) webClient = new WebClient();
            webClient.DownloadStringCompleted += WebClient_OnDownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(updateURL));
        }

        public void UpdateCurrentApp()
        {
            Properties.Settings.Default.UpdateAvailable = false;
            Properties.Settings.Default.Save();

            if (File.Exists(downloadedFileName))
            {
                // Checa se arquivo de atualização foi baixado no disco
                RunInstaller(downloadedFileName, latestVersion.silentInstallArgs);
            }
            else
            {
                MessageHelper.ShowError("O arquivo de atualização não foi encontrado! Reinicie o aplicativo e tente novamente ou visite locutordahora.unijui.edu.br.");
            }
        }
        #endregion

        #region Classes Auxiliares
        public class VersionInfo
        {
            public string appID;
            public string version;
            public string downloadURL;
            public string silentInstallArgs;
            public string versionNotes;
        }
        #endregion
    }
}
