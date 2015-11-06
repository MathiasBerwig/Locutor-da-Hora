using System.Windows.Controls;
using System.Windows.Navigation;
using Locutor_da_Hora.Pages;
using Locutor_da_Hora.Utils;

namespace Locutor_da_Hora.Windows
{
    /// <summary>
    /// Lógica de interação para o MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Construtores Públicos
        public MainWindow()
        {
            // Inicializa os componentes gráficos
            InitializeComponent();

            // Define a instância. 
            // É preciso realizar essa referência no Construtor devido à configuração padrão da aplicação (construtor público)
            Instance = this;

            // Define os eventos da janela principal
            FramePrincipal.LoadCompleted += FramePrincipal_OnLoadCompleted;
            
            // Abre a página de Boas Vindas
            FramePrincipal.Navigate(BoasVindas.Instance);

            // De-serializa as Locuções
            GerenciadorLocucoes.Instance.CarregarLocucoes();
        }
        #endregion

        #region Membros Públicos
        public static MainWindow Instance { get; private set; }
        #endregion

        #region Métodos Privados
        private void FramePrincipal_OnLoadCompleted(object sender, NavigationEventArgs navigationEventArgs)
        {
            var pageTitle = ((Page) FramePrincipal.Content).Title;

            if (!string.IsNullOrWhiteSpace(pageTitle))
                GoogleAnalyticsTracker.Instance.TrackScreen(pageTitle);
        }       
        #endregion

        #region Métodos Públicos
        public bool AbrirPagina(Page paginaDestino)
        {
            return FramePrincipal.NavigationService.Navigate(paginaDestino);
        }
        #endregion
    }
}