using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Locutor_da_Hora.Audio;
using Locutor_da_Hora.Model;
using Locutor_da_Hora.Pages.SubPages;
using Locutor_da_Hora.Utils;
using Locutor_da_Hora.Windows;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Locutor_da_Hora.Pages
{
    /// <summary>
    /// Lógica de interação para o Edicao.xaml
    /// </summary>
    public partial class Edicao : INotifyPropertyChanged
    {
        #region Membros Privados
        private bool podeGravarNovamente;
        private bool processando;
        private static Edicao instance;
        #endregion

        #region Construtores Privados
        private Edicao()
        {
            // Inicializa os componentes gráficos
            InitializeComponent();

            // Define o contexto da aplicação
            DataContext = this;

            FramePrincipal.LoadCompleted += FramePrincipal_LoadCompleted;
        }
        #endregion

        #region Membros Públicos
        public static Edicao Instance => instance ?? (instance = new Edicao());

        /// <summary>
        /// Indica se o usuário pode retornar à tela anterior para gravar novamente. Utilizado no Binding da Visibility do botão "BtGravarNovamente".
        /// </summary>
        public bool PodeGravarNovamente
        {
            get { return podeGravarNovamente; }
            set { SetField(ref podeGravarNovamente, value, nameof(PodeGravarNovamente)); }
        }

        /// <summary>
        /// Indica se o usuário pode enviar a gravação atual via e-mail. Utilizado no Binding da Visibility do botão "BtEnviarViaEmail".
        /// </summary>
        public bool PodeEnviarViaEmail => App.ModoExposicao;

        /// <summary>
        /// Indica se existe algum processamento sendo executado no momento. Utilizado no Binding da Visibility do grid "Carregando".
        /// </summary>
        public bool Processando
        {
            get { return processando; }
            set { SetField(ref processando, value, nameof(Processando)); }
        }

        public NAudioEngine EngineAtivo
        {
            get { return EditarAudio.Instance.EngineAtivo; }
            set { EditarAudio.Instance.EngineAtivo = value; }
        }

        public NAudioEngine EngineVoz => EditarAudio.Instance.EngineVoz;

        public NAudioEngine EngineTrilhaSonora => EditarAudio.Instance.EngineTrilhaSonora;
        #endregion

        #region Métodos Privados
        private void FramePrincipal_LoadCompleted(object sender, NavigationEventArgs e)
        {
            // Registra a página aberta no momento
            string pageTitle = ((Page)FramePrincipal.Content).Title;
            if (!string.IsNullOrWhiteSpace(pageTitle))
                GoogleAnalyticsTracker.Instance.TrackScreen(pageTitle);
        }
        #endregion

        #region Eventos
        #region Navegação
        private void BtGravarNovamente_Click(object sender, RoutedEventArgs e)
        {
            // Navega o Frame para a página Seleção de Locução
            MainWindow.Instance.AbrirPagina(SelecaoLocucao.Instance);

            // Interrompe a reprodução
            EditarAudio.Instance.EngineVoz.StopAndClear();
            EditarAudio.Instance.EngineTrilhaSonora.StopAndClear();
        }

        private void BtExportar_Click(object sender, RoutedEventArgs e)
        {
            // Cria o diálogo 
            var dialog = new SaveFileDialog { Filter = Properties.Resources.Dialog_Filter_MP3 };
            var result = dialog.ShowDialog();
            if (result != true) return;

            var volumeVoz = (float)EditarAudio.Instance.SliderVolumeVoz.Value;
            var arquivoVoz = EditarAudio.Instance.EngineVoz.FileName;
            var volumeTrilha = (float)EditarAudio.Instance.SliderVolumeTrilha.Value;
            var arquivoTrilha = EditarAudio.Instance.EngineTrilhaSonora.FileName;
            var novoArquivo = dialog.FileName;

            // Interrompe a reprodução
            EditarAudio.Instance.EngineVoz.Stop();
            EditarAudio.Instance.EngineTrilhaSonora.Stop();

            // Inicia a exportação em uma nova tarefa
            Task.Factory.StartNew(() =>
            {
                Processando = true;

                try
                {
                    // Transforma o período selecionado em uma nova Stream                
                    MemoryStream novaStream = Audio.Edicao.MesclarMp3(arquivoTrilha, volumeTrilha, arquivoVoz, volumeVoz);

                    // Exporta
                    Audio.Edicao.ExportarMp3(novaStream, novoArquivo, true);
                }
                catch (Exception exception)
                {
                    var erro = Properties.Resources.Exception_ExportarAudio;
                    GoogleAnalyticsTracker.Instance.TrackException(exception.Message, true);
                    MessageHelper.ShowError(erro);
                }

                Processando = false;
            });
        }

        private void BtEnviarViaEmail_Click(object sender, RoutedEventArgs e)
        {
            // De-serializa o template de envio
            string mailTemplateFile = Properties.Resources.Arquivo_TemplateEmail;
            MailTemplate mailTemplate = SerializationHelper.ReadFromXmlFile<MailTemplate>(mailTemplateFile);
            EnviarEmail.Instance.MailTemplate = mailTemplate;

            EnviarEmail.Instance.CarregarBanner(mailTemplate.BannerImagePath);
            FramePrincipal.Navigate(EnviarEmail.Instance);
        }

        private void BtVoltarInicio_Click(object sender, RoutedEventArgs e)
        {
            // Navega o Frame para a página Boas Vindas
            MainWindow.Instance.AbrirPagina(BoasVindas.Instance);

            // Interrompe a reprodução
            EditarAudio.Instance.EngineVoz.StopAndClear();
            EditarAudio.Instance.EngineTrilhaSonora.StopAndClear();
        }
        #endregion
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
