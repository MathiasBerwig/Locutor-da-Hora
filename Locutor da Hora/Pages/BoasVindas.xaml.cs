using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Locutor_da_Hora.Windows;
using Locutor_da_Hora.Utils;

namespace Locutor_da_Hora.Pages
{
    /// <summary>
    /// Lógica de interação para o BoasVindas.xaml
    /// </summary>
    public partial class BoasVindas : INotifyPropertyChanged
    {
        #region Membros Privados
        private static BoasVindas instance;
        private const string URL_AVALIACAO = "http://locutordahora.unijui.edu.br/avaliacao-do-aplicativo";
        #endregion

        #region Construtores Privados
        private BoasVindas()
        {
            // Inicializa os componentes gráficos
            InitializeComponent();

            // Define o contexto da aplicação
            DataContext = this;            
        }
        #endregion

        #region Membros Públicos
        public static BoasVindas Instance => instance ?? (instance = new BoasVindas());

        /// <summary>
        /// Indica se o usuário pode avaliar a aplicação. Utilizado no binding do grid de avaliação.
        /// </summary>
        public bool PodeAvaliar
        {
            get { return Properties.Settings.Default.PodeAvaliar && Properties.Settings.Default.RecordingCount > 5 && !AtualizacaoDisponivel; }
            set { OnPropertyChanged(nameof(PodeAvaliar)); }
        }

        /// <summary>
        /// Indica se uma atualização foi baixada e está disponível. Utilizado no binding da notificação.
        /// </summary>
        public bool AtualizacaoDisponivel
        {
            get { return Properties.Settings.Default.UpdateDownloaded; }
            set { OnPropertyChanged(nameof(AtualizacaoDisponivel)); }
        }
        #endregion

        #region Eventos Bt_Click
        private void BtSair_Click(object sender, RoutedEventArgs e)
        {
            //Fecha a aplicação
            Application.Current.Shutdown();
        }

        private void BtIniciar_Click(object sender, RoutedEventArgs e)
        {
            // Navega o Frame para a página Identificação
            MainWindow.Instance.AbrirPagina(Identificacao.Instance);            
        }

        private void BtModoEdicao_Click(object sender, RoutedEventArgs e)
        {
            // Desativa o botão Gravar Novamente
            Edicao.Instance.PodeGravarNovamente = false;

            // Navega o Frame para a página Edição
            MainWindow.Instance.AbrirPagina(Edicao.Instance);
        }

        private void BtAvaliar_Click(object sender, RoutedEventArgs e)
        {
            // Abre o link de avaliação do aplicativo no navegador padrão
            System.Diagnostics.Process.Start(URL_AVALIACAO);

            // Desabilita a opção de avaliação, fazendo com que o botão avaliar não esteja mais disponível na próxima inicialização
            Properties.Settings.Default.PodeAvaliar = false;
            Properties.Settings.Default.Save();

            GoogleAnalyticsTracker.Instance.TrackEvent("Interações", "Avaliar", null, null);
        }

        private void BtAtualizar_Click(object sender, RoutedEventArgs e)
        {
            // Executa o instalador e fecha o Locutor da Hora
            UpdateHelper.Instance.UpdateCurrentApp();
        }
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
