using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Locutor_da_Hora.Model;
using Locutor_da_Hora.Utils;
using Locutor_da_Hora.Windows;
using Button = System.Windows.Controls.Button;

namespace Locutor_da_Hora.Pages.SubPages
{
    /// <summary>
    /// Interaction logic for ListaLocucoes.xaml
    /// </summary>
    public partial class ListaLocucoes
    {
        #region Membros Privados
        private static ListaLocucoes instance;
        #endregion

        #region Construtores Privados
        private ListaLocucoes()
        {
            // Inicializa os componentes gráficos
            InitializeComponent();

            // Define o contexto da aplicação
            DataContext = this;
        }
        #endregion

        #region Membros Públicos
        public static ListaLocucoes Instance => instance ?? (instance = new ListaLocucoes());

        public ObservableCollection<Locucao> Locucoes { get; } = GerenciadorLocucoes.Instance.Locucoes;        
        
        /// <summary>        
        /// Calcula a quantidade de linhas de modo a não cortar os botões de locução. Utiliza a seguinte fórmula:
        /// linhas  = (Tamanho da Tela - Margens de Outros Componentes) / (Altura do Botão + Margem)
        /// </summary>
        public double ScrollViewer_Height
        {
            get
            {
                const int itemHeight = 200;
                int linhas = (int)((SystemParameters.PrimaryScreenHeight - 353) / itemHeight);
                return linhas * itemHeight;
            }
        }        
        #endregion

        #region Eventos
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private void BtnLocucao_Click(object sender, RoutedEventArgs e)
        {
            Locucao locucao = (sender as Button).Tag as Locucao;            

            // Adicionar Locução
            if (locucao.UniqueName.Equals(GerenciadorLocucoes.ADICIONAR_LOCUCAO))
            {
                // Define o título da página de Edição de Locução
                EditarLocucao.Instance.TituloPagina = Properties.Resources.Titulo_AdicionarLocucao;
                // Cria a nova Locução e a adiciona à lista
                Locucao novaLocucao = new Locucao {Titulo = Properties.Resources.Titulo_NovaLocucao, ReadOnly = App.DebugMode};
                GerenciadorLocucoes.Instance.Locucoes.Add(novaLocucao);
                // Vincula a nova Locução à Locucao do EditarLocucao
                EditarLocucao.Instance.Locucao = novaLocucao;
                // Navega o Frame para a Editar Locução
                SelecaoLocucao.Instance.FramePrincipal.Navigate(EditarLocucao.Instance);
            }
            else
            // Modo de Edição ativado
            if (SelecaoLocucao.Instance.PodeCancelarEdicao)
            {
                // Locução somente leitura. Impede a edição
                if (locucao.ReadOnly && !App.DebugMode) return;
                // Define o título da página de Edição de Locução
                EditarLocucao.Instance.TituloPagina = Properties.Resources.Titulo_EditarLocucao;
                // Vincula a Locução do botão à Locucao do EditarLocucao
                EditarLocucao.Instance.Locucao = locucao;
                // Navega o SelecaoLocucao.FramePrincipal para EditarLocucao
                SelecaoLocucao.Instance.FramePrincipal.Navigate(EditarLocucao.Instance);
            }
            else
            {
                // Vincula a Locução do botão à Locucao da Gravação
                Gravacao.Instance.Locucao = locucao;
                // Navega o Frame Principal para a Gravação
                MainWindow.Instance.AbrirPagina(Gravacao.Instance);
            }
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private void BtnExcluir_Click(object sender, RoutedEventArgs e)
        {
            Locucao locucao = (sender as Button).Tag as Locucao;

            if (locucao.UniqueName.Equals(GerenciadorLocucoes.ADICIONAR_LOCUCAO)) return;

            try
            {
                // Exclui os arquivos de ícone e trilha sonora
                File.Delete(locucao.LocalIcone);
                File.Delete(locucao.LocalTrilhaSonora);
            }
            catch
            {

            }

            // Exclui a locução da lista
            GerenciadorLocucoes.Instance.Locucoes.Remove(locucao);
        }

        private void MouseEnterAnimacao(object sender, RoutedEventArgs e)
        {
            Storyboard storyboardX = new Storyboard();
            Storyboard storyboardY = new Storyboard();

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            ((Button)sender).RenderTransformOrigin = new Point(0.5, 0.5);
            ((Button)sender).RenderTransform = scale;

            // Define as propriedades da animação no Eixo X
            DoubleAnimation growAnimationX = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                From = 1,
                To = 1.1
            };
            storyboardX.Children.Add(growAnimationX);

            // Define as propriedades da animação no Eixo Y
            DoubleAnimation growAnimationY = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                From = 1,
                To = 1.1
            };
            storyboardX.Children.Add(growAnimationY);

            Storyboard.SetTargetProperty(growAnimationX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(growAnimationY, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(growAnimationX, ((Button) sender));
            Storyboard.SetTarget(growAnimationY, ((Button) sender));

            // Inicia a animação
            storyboardX.Begin();
            storyboardY.Begin();
        }

        private void MouseLeaveAnimacao(object sender, RoutedEventArgs e)
        {
            Storyboard storyboardX = new Storyboard();
            Storyboard storyboardY = new Storyboard();

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            ((Button)sender).RenderTransformOrigin = new Point(0.5, 0.5);
            ((Button)sender).RenderTransform = scale;

            // Define as propriedades da animação no Eixo X
            DoubleAnimation growAnimationX = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                From = 1.1,
                To = 1
            };
            storyboardX.Children.Add(growAnimationX);

            // Define as propriedades da animação no Eixo Y
            DoubleAnimation growAnimationY = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                From = 1.1,
                To = 1
            };
            storyboardX.Children.Add(growAnimationY);

            Storyboard.SetTargetProperty(growAnimationX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(growAnimationY, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(growAnimationX, ((Button) sender));
            Storyboard.SetTarget(growAnimationY, ((Button) sender));

            // Inicia a animação
            storyboardX.Begin();
            storyboardY.Begin();
        }
        #endregion
    }
}
