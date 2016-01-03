using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using Locutor_da_Hora.Model;
using Locutor_da_Hora.Utils;
using Microsoft.Win32;

namespace Locutor_da_Hora.Pages.SubPages
{
    /// <summary>
    /// Interaction logic for EditarLocucao.xaml
    /// </summary>
    public partial class EditarLocucao : INotifyPropertyChanged
    {
        #region Membros Privados
        private static EditarLocucao instance;
        private string tituloPagina;
        private Locucao locucao;
        #endregion
        
        #region Construtores Privados
        private EditarLocucao()
        {
            // Inicializa os componentes gráficos
            InitializeComponent();

            // Define o contexto da aplicação
            DataContext = this;
        }
        #endregion

        #region Membros Públicos
        public static EditarLocucao Instance => instance ?? (instance = new EditarLocucao());

        public Locucao Locucao
        {
            get { return locucao; }
            set
            {
                SetField(ref locucao, value, nameof(Locucao));
                BtRemoverIcone.Visibility = value?.LocalIcone != null && File.Exists(value.LocalIcone) ? Visibility.Visible : Visibility.Hidden;
                BtRemoverTrilhaSonora.Visibility = value?.LocalTrilhaSonora != null && File.Exists(value.LocalTrilhaSonora) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public string TituloPagina
        {
            get { return tituloPagina; }
            set { SetField(ref tituloPagina, value, nameof(TituloPagina)); }
        }
        #endregion

        #region Eventos Bt_Click
        private void BtSelecionarIcone_Click(object sender, RoutedEventArgs e)
        {
            // Abre o diálogo de seleção do arquivo
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = Properties.Resources.Dialog_Filter_PNG,
                Title = Properties.Resources.Dialog_SelecionarIcone,
                Multiselect = false,
            };

            // Checa o resultado do diálogo
            if (dialog.ShowDialog() != true) return;
            string fileName = dialog.FileName;

            // Verifica o arquivo de ícone. Se PNG, então copia
            if (Path.GetExtension(fileName) == ".png")
            {
                // Altera a resolução da imagem (125x125px) e salva o arquivo
                ImageHandler.Save(new Bitmap(fileName), 125, 125, 100, locucao.LocalIcone);                

                // Notifica alteração
                locucao.NotifyPropertyChanged(nameof(Locucao.Icone));

                // Mostra o botão Remover Ícone
                BtRemoverIcone.Visibility = Visibility.Visible;
            }
            else
            {
                MessageHelper.ShowError(Properties.Resources.Exception_ArquivoInvalido);
            }
        }

        private void BtSelecionarTrilhaSonora_Click(object sender, RoutedEventArgs e)
        {
            // Abre o diálogo de seleção do arquivo
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = Properties.Resources.Dialog_Filter_MP3,
                Title = Properties.Resources.Dialog_SelecionarAudio,
                Multiselect = false,
            };

            // Checa o resultado do diálogo e então converte a trilha, caso necessário
            if (dialog.ShowDialog() != true) return;
            string fileName = dialog.FileName;

            if (fileName.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase))
            {
                // Copia o arquivo
                File.Copy(fileName, locucao.LocalTrilhaSonora, true);

                // Mostra o botão Remover Trilha Sonora
                BtRemoverTrilhaSonora.Visibility = Visibility.Visible;
            }
            else
            {
                MessageHelper.ShowError(Properties.Resources.Exception_ArquivoInvalido);
            }
        }

        private void BtRemoverTrilhaSonora_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                File.Delete(locucao.LocalTrilhaSonora);
                BtRemoverTrilhaSonora.Visibility = Visibility.Hidden;
            }
            catch (Exception)
            {
                MessageHelper.ShowError(Properties.Resources.Exception_ExcluirTrilha);
            }
        }

        private void BtRemoverIcone_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                File.Delete(locucao.LocalIcone);
                BtRemoverIcone.Visibility = Visibility.Hidden;
                locucao.NotifyPropertyChanged(nameof(Locucao.Icone));
            }
            catch (Exception)
            {
                MessageHelper.ShowError(Properties.Resources.Exception_ExcluirIcone);
            }
        }

        private void BtDiminuirTamanhoTexto_Click(object sender, RoutedEventArgs e)
        {
            if (SlTamanhoTexto.Value > SlTamanhoTexto.Minimum) SlTamanhoTexto.Value--;
        }

        private void BtAumentarTamanhoTexto_Click(object sender, RoutedEventArgs e)
        {
            if (SlTamanhoTexto.Value < SlTamanhoTexto.Maximum) SlTamanhoTexto.Value++;
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
