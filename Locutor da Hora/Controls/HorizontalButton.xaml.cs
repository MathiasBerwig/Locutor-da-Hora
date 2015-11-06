using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Locutor_da_Hora.Controls
{
    /// <summary>
    /// Interaction logic for HorizontalButton.xaml
    /// </summary>
    public partial class HorizontalButton : UserControl
    {
        #region Construtores Públicos
        public HorizontalButton()
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
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(HorizontalButton), new UIPropertyMetadata(null));
        #endregion

        #region Legenda Botão
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HorizontalButton), new UIPropertyMetadata(null));
        #endregion
        
        #region Legenda Botão
        public string ToolTip
        {
            get { return (string)GetValue(ToolTipProperty); }
            set { SetValue(ToolTipProperty, value); }
        }

        public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register("ToolTip", typeof(string), typeof(MenuButton), new UIPropertyMetadata(null));
        #endregion
        #endregion
    }
}
