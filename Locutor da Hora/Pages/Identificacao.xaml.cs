using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Locutor_da_Hora.Model;
using Locutor_da_Hora.Pages.SubPages;
using Locutor_da_Hora.Windows;

namespace Locutor_da_Hora.Pages
{
    /// <summary>
    /// Lógica de interação para o Identificacao.xaml
    /// </summary>
    public partial class Identificacao : INotifyPropertyChanged
    {
        #region Membros Privados
        private int errors;
        private Usuario usuario;
        private bool podeAvancar;
        private static Identificacao instance;
        #endregion

        #region Construtores Privados
        private Identificacao()
        {
            // Inicializa os componentes gráficos
            InitializeComponent();

            // Define o contexto da aplicação
            DataContext = this;

            // Direciona o foco para o primeiro componente da tela
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            usuario = new Usuario();
        }
        #endregion

        #region Membros Públicos
        public static Identificacao Instance => instance ?? (instance = new Identificacao());

        public Usuario Usuario
        {
            get { return usuario; }
            set { SetField(ref usuario, value, "Usuario"); }
        }

        public bool PodeAvancar
        {
            get { return podeAvancar; }
            set { SetField(ref podeAvancar, value, "PodeAvancar"); }
        }
        #endregion

        #region Métodos Privados
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                errors++;
            else
                errors--;
            PodeAvancar = errors == 0;
        }
        #endregion

        #region Métodos Públicos

        #endregion

        #region Eventos de Componentes
        private void BtVoltar_Click(object sender, RoutedEventArgs e)
        {
            // Navega o Frame para a página de Boas Vindas
            MainWindow.Instance.AbrirPagina(BoasVindas.Instance);
        }

        private void BtAvancar_Click(object sender, RoutedEventArgs e)
        {
            // Mostra a Lista de Padrões como exibição padrão
            SelecaoLocucao.Instance.FramePrincipal.Navigate(ListaLocucoes.Instance);

            // Navega o Frame para a página de Seleção de Locução
            MainWindow.Instance.AbrirPagina(SelecaoLocucao.Instance);            
        }

        private void Tb_KeyDown(object sender, KeyEventArgs e)
        {
            // Verifica se existem erros e a tecla pressionada é Enter. Neste caso avança para a próxima aba
            if (errors == 0 && e.Key == Key.Enter) BtAvancar_Click(null, null);
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
