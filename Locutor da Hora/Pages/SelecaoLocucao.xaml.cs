using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Locutor_da_Hora.Pages.SubPages;
using Locutor_da_Hora.Windows;
using Locutor_da_Hora.Utils;

namespace Locutor_da_Hora.Pages
{
    /// <summary>
    /// Interaction logic for SelecaoLocucao.xaml
    /// </summary>
    public partial class SelecaoLocucao : INotifyPropertyChanged
    {
        #region Membros Privados

        private static SelecaoLocucao instance;
        private bool podeScroll = true;
        private bool podeEditar = true;
        private bool podeExcluir;
        private bool podeCancelarEdicao;
        private bool podeConfigurar;

        #endregion

        #region Construtores Privados

        private SelecaoLocucao()
        {
            // Define o contexto da aplicação
            DataContext = this;

            // Inicializa os componentes gráficos
            InitializeComponent();

            FramePrincipal.LoadCompleted += FramePrincipal_LoadCompleted;
        }

        #endregion

        #region Membros Públicos

        public static SelecaoLocucao Instance => instance ?? (instance = new SelecaoLocucao());

        /// <summary>
        /// Indica se o usuário pode utilizar o Scroll (BtSubir/BtDescer). Utilizado no Binding da Visibility do botões "BtSubir" e "BtDescer".
        /// </summary>
        public bool PodeScroll
        {
            get { return podeScroll; }
            set { SetField(ref podeScroll, value, "PodeScroll"); }
        }

        /// <summary>
        /// Indica se o usuário pode iniciar o modo de edição. Utilizado no Binding da Visibility dos botões "BtVoltar" e "BtEditarLocucoes".
        /// </summary>
        public bool PodeEditar
        {
            get { return podeEditar; }
            set { SetField(ref podeEditar, value, "PodeEditar"); }
        }

        /// <summary>
        /// Indica que o modo de edição está ativo e se o usuário pode cancelar a edição. Utilizado no Binding da Visibility do botão "BtnInterromperEdicao" e botões de exclusão de locução.
        /// </summary>
        public bool PodeCancelarEdicao
        {
            get { return podeCancelarEdicao; }
            set { SetField(ref podeCancelarEdicao, value, "PodeCancelarEdicao"); }
        }

        /// <summary>
        /// Indica se o usuário pode abrir as configurações. Utilizado no Binding da Visibility do botão "BtConfiguracoes".
        /// </summary>
        public bool PodeConfigurar
        {
            get { return podeConfigurar; }
            set { SetField(ref podeConfigurar, value, "PodeConfigurar"); }
        }

        /// <summary>
        /// Indica se o usuário pode excluir locuções. Utilizado no Binding da Visibility dos botões "BtExcluir" (ListaLocucoes.xaml).
        /// </summary>
        public bool PodeExcluir
        {
            get { return podeExcluir; }
            set { SetField(ref podeExcluir, value, "PodeExcluir"); }
        }

        #endregion

        #region Métodos Privados

        private void FramePrincipal_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Scroll habilitado somente na Lista de Locuções
            PodeScroll = FramePrincipal.Content.Equals(ListaLocucoes.Instance);

            // Botão Editar habilitado somente na Lista de Locuções
            PodeEditar = FramePrincipal.Content.Equals(ListaLocucoes.Instance);

            // Botão Cancelar Edição habilitado somente durante a Edição de Locução
            PodeCancelarEdicao = FramePrincipal.Content.Equals(EditarLocucao.Instance);

            // Botão Configurações habilitado somente na Lista de Locuções
//            PodeConfigurar = FramePrincipal.Content.Equals(ListaLocucoes.Instance);

            // Registra a página aberta no momento
            string pageTitle = ((Page) FramePrincipal.Content).Title;
            if (!String.IsNullOrWhiteSpace(pageTitle))
                GoogleAnalyticsTracker.Instance.TrackScreen(pageTitle);
        }

        #endregion

        #region Métodos Públicos

        #endregion

        #region Eventos Bt_Click

        private void BtVoltar_Click(object sender, RoutedEventArgs e)
        {
            // Navega o Frame para a página Identificação
            MainWindow.Instance.AbrirPagina(Identificacao.Instance);
        }

        private void BtEditarLocucoes_Click(object sender, RoutedEventArgs e)
        {
            PodeEditar = false;
            PodeCancelarEdicao = true;
//            PodeConfigurar = false;
            PodeExcluir = true;
        }

        private void BtnGridinterromperEdicao_Click(object sender, RoutedEventArgs e)
        {
            // Navega o Frame principal da tela de Seleção de Locução para a Lista de Locuções
            FramePrincipal.NavigationService.Navigate(ListaLocucoes.Instance);
            // Salva as locuções
            GerenciadorLocucoes.Instance.SalvarLocucoes();

            PodeCancelarEdicao = false;
            PodeExcluir = false;
            PodeEditar = true;
//            PodeConfigurar = true;
        }

        private void BtConfiguracoes_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtSubir_Click(object sender, RoutedEventArgs e)
        {
            ListaLocucoes.Instance.ScrollViewer.PageUp();
        }

        private void BtBaixar_Click(object sender, RoutedEventArgs e)
        {
            ListaLocucoes.Instance.ScrollViewer.PageDown();
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