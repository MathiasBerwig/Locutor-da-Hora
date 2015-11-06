using System.Windows;
using System.Windows.Media;

namespace Locutor_da_Hora.Controls
{
    /// <summary>
    /// Interaction logic for MenuButton.xaml
    /// </summary>
    public partial class MenuButton
    {
        #region Construtores Públicos
        public MenuButton()
        {
            // Inicializa os componentes gráficos
            InitializeComponent();
        }
        #endregion

        #region Membros Públicos
        #region Evento Click
        public event RoutedEventHandler Click
        {
            add { button.Click += value; }
            remove { button.Click -= value; }
        }
        #endregion

        #region Imagem do Botão
        public ImageSource Icon
        {
            get { return (ImageSource) GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof (ImageSource), typeof (MenuButton), new UIPropertyMetadata(null));
        #endregion

        #region Legenda Botão
        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string), typeof (MenuButton), new UIPropertyMetadata(null));
        #endregion
        #endregion
    }
}