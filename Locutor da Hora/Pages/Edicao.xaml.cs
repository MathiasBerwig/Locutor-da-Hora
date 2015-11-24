using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Locutor_da_Hora.Audio;
using Locutor_da_Hora.Utils;
using Locutor_da_Hora.Windows;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Locutor_da_Hora.Pages
{
    /// <summary>
    /// Lógica de interação para o Edicao.xaml
    /// </summary>
    public partial class Edicao : INotifyPropertyChanged
    {
        #region Membros Privados        
        private NAudioEngine engineVoz;
        private NAudioEngine engineTrilhaSonora;
        private NAudioEngine engineAtivo;
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

            // Registra os WaveForms
            WaveformVoz.RegisterSoundPlayer(EngineVoz);
            WaveformTrilhaSonora.RegisterSoundPlayer(EngineTrilhaSonora);
        }
        #endregion

        #region Membros Públicos
        public static Edicao Instance => instance ?? (instance = new Edicao());

        public NAudioEngine EngineVoz => engineVoz ?? (engineVoz = new NAudioEngine());

        public NAudioEngine EngineTrilhaSonora => engineTrilhaSonora ?? (engineTrilhaSonora = new NAudioEngine());

        public NAudioEngine EngineAtivo
        {
            get { return engineAtivo; }
            set { SetField(ref engineAtivo, value, nameof(EngineAtivo)); }
        }

        /// <summary>
        /// Indica se o usuário pode retornar à tela anterior para gravar novamente. Utilizado no Binding da Visibility do botão "BtGravarNovamente".
        /// </summary>
        public bool PodeGravarNovamente
        {
            get { return podeGravarNovamente; }
            set { SetField(ref podeGravarNovamente, value, nameof(PodeGravarNovamente)); }
        }

        /// <summary>
        /// Indica se existe algum processamento sendo executado no momento. Utilizado no Binding da Visibility do grid "Carregando".
        /// </summary>
        public bool Processando
        {
            get { return processando; }
            set { SetField(ref processando, value, nameof(Processando)); }
        }
        #endregion

        #region Métodos Privados
        private void DefinirEngineAtivo(object sender)
        {
            // Verifica o objeto que invocou o método e então define o EngineAtivo
            if (sender.Equals(WaveformVoz))
            {
                EngineAtivo = EngineVoz;
            }
            else if (sender.Equals(WaveformTrilhaSonora))
            {
                EngineAtivo = EngineTrilhaSonora;
            }
        }
        #endregion

        #region Eventos
        #region Navegação
        private void BtGravarNovamente_Click(object sender, RoutedEventArgs e)
        {
            // Navega o Frame para a página Seleção de Locução
            MainWindow.Instance.AbrirPagina(SelecaoLocucao.Instance);

            // Interrompe a reprodução
            EngineVoz.StopAndClear();
            EngineTrilhaSonora.StopAndClear();
        }

        private void BtExportar_Click(object sender, RoutedEventArgs e)
        {
            // Cria o diálogo 
            var dialog = new SaveFileDialog { Filter = Properties.Resources.Dialog_Filter_MP3 };
            var result = dialog.ShowDialog();
            if (result != true) return;

            var volumeVoz = (float)SliderVolumeVoz.Value;
            var arquivoVoz = EngineVoz.FileName;
            var volumeTrilha = (float)SliderVolumeTrilha.Value;
            var arquivoTrilha = EngineTrilhaSonora.FileName;
            var novoArquivo = dialog.FileName;

            // Interrompe a reprodução
            EngineVoz.Stop();
            EngineTrilhaSonora.Stop();

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

        private void BtVoltarInicio_Click(object sender, RoutedEventArgs e)
        {
            // Navega o Frame para a página Boas Vindas
            MainWindow.Instance.AbrirPagina(BoasVindas.Instance);

            // Interrompe a reprodução
            EngineVoz.StopAndClear();
            EngineTrilhaSonora.StopAndClear();
        }
        #endregion

        #region Waveform
        private void Waveform_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DefinirEngineAtivo(sender);
        }

        private void Waveform_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Reproduz o Engine Ativo
            EngineAtivo?.Play();
        }

        #region Menu de Contexto
        private void Waveform_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            DefinirEngineAtivo(sender);
        }

        private void MiCarregar_Click(object sender, RoutedEventArgs e)
        {

            // Abre o diálogo de seleção do arquivo
            var dialog = new OpenFileDialog()
            {
                Filter = Properties.Resources.Dialog_Filter_MP3,
                Title = Properties.Resources.Dialog_SelecionarAudio,
                Multiselect = false,
            };

            // Checa o resultado do diálogo
            if (dialog.ShowDialog() != true) return;
            var filename = dialog.FileName;

            if (filename.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase))
            {
                EngineAtivo.OpenFile(filename);
            }
            else
            {
                var erro = Properties.Resources.Exception_ArquivoInvalido;
                MessageHelper.ShowWarning(erro);
            }
        }

        private void MiRemoverTrecho_Click(object sender, RoutedEventArgs e)
        {
            // Coleta os tempos selecionados
            var inicioSelecao = EngineAtivo.SelectionBegin;
            var fimSelecao = EngineAtivo.SelectionEnd;

            // Libera os recursos do Engine Ativo
            EngineAtivo.StopAndClear();
            
            var novoArquivo = Environment.CurrentDirectory + Properties.Resources.PastaTemporaria_MP3 + @"\" + Guid.NewGuid() + ".mp3";
            var arquivoAntigo = EngineAtivo.FileName;

            // Inicia a remoção em uma nova tarefa e depois carrega o arquivo gerado
            Task.Factory.StartNew(() =>
            {
                Processando = true;

                try
                {
                    // Transforma o período selecionado em uma nova Stream
                    MemoryStream novaStream = Audio.Edicao.RemoverTrechoMp3(arquivoAntigo, inicioSelecao, fimSelecao);

                    // Exporta
                    Audio.Edicao.ExportarMp3(novaStream, novoArquivo, true);
                }
                catch (Exception exception)
                {
                    var erro = Properties.Resources.Exception_ExportarAudio;
                    GoogleAnalyticsTracker.Instance.TrackException(exception.Message, true);
                    MessageHelper.ShowError(erro);
                }
            }).ContinueWith((t1) =>
            {
                // Carrega a nova Stream a partir da thread principal (UI)
                EngineAtivo.OpenFile(novoArquivo);

                Processando = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void MiCortar_Click(object sender, RoutedEventArgs e)
        {
            // Coleta os tempos selecionados
            var inicioSelecao = EngineAtivo.SelectionBegin;
            var fimSelecao = EngineAtivo.SelectionEnd;

            // Libera os recursos do Engine Ativo
            EngineAtivo.StopAndClear();

            var novoArquivo = Environment.CurrentDirectory + Properties.Resources.PastaTemporaria_MP3 + @"\" + Guid.NewGuid() + ".mp3";
            var arquivoAntigo = EngineAtivo.FileName;

            // Inicia a remoção em uma nova tarefa e depois carrega o arquivo gerado
            Task.Factory.StartNew(() =>
            {
                Processando = true;

                try
                {
                    // Transforma o período selecionado em uma nova Stream
                    MemoryStream novaStream = Audio.Edicao.CortarMp3(arquivoAntigo, inicioSelecao, fimSelecao);

                    // Exporta
                    Audio.Edicao.ExportarMp3(novaStream, novoArquivo, true);
                }
                catch (Exception exception)
                {
                    var erro = Properties.Resources.Exception_ExportarAudio;
                    GoogleAnalyticsTracker.Instance.TrackException(exception.Message, true);
                    MessageHelper.ShowError(erro);
                }
                
            }).ContinueWith((t1) =>
            {
                // Carrega a nova Stream a partir da thread principal (UI)
                EngineAtivo.OpenFile(novoArquivo);

                Processando = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void MiExportar_Click(object sender, RoutedEventArgs e)
        {
            // Cria o diálogo 
            var dialog = new SaveFileDialog { Filter = Properties.Resources.Dialog_Filter_MP3 };
            var result = dialog.ShowDialog();
            if (result != true) return;

            // Interrompe a reprodução            
            EngineAtivo.Stop();

            // Inicia a remoção em uma nova tarefa e depois carrega o arquivo gerado
            Task.Factory.StartNew(() =>
            {
                Processando = true;

                try
                {
                    if (EngineAtivo.CanCut)
                    {
                        // Coleta os tempos selecionados
                        var inicioSelecao = EngineAtivo.SelectionBegin;
                        var fimSelecao = EngineAtivo.SelectionEnd;
                        
                        // Transforma o período selecionado em uma nova Stream
                        MemoryStream novaStream = Audio.Edicao.CortarMp3(EngineAtivo.FileName, inicioSelecao, fimSelecao);

                        // Exporta
                        Audio.Edicao.ExportarMp3(novaStream, dialog.FileName, true);
                    }
                    else
                    {
                        File.Copy(EngineAtivo.FileName, dialog.FileName, true);
                    }
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

        #endregion Menu de Contexto
        #endregion Waveform

        #region Reprodução
        private void BtReproduzir_Click(object sender, RoutedEventArgs e)
        {
            // Reproduz o Engine Ativo
            EngineAtivo?.Play();
        }

        private void BtReproduzir_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Reproduz os Engines de Voz e Trilha Sonora
            EngineVoz?.Play();
            EngineTrilhaSonora?.Play();
        }

        private void BtPausar_Click(object sender, RoutedEventArgs e)
        {
            // Pausa o Engine Ativo
            EngineAtivo?.Pause();
        }

        private void BtPausar_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Pausa os Engines de Voz e Trilha Sonora
            EngineVoz?.Pause();
            EngineTrilhaSonora?.Pause();
        }

        private void BtParar_Click(object sender, RoutedEventArgs e)
        {
            // Para o Engine Ativo
            EngineAtivo?.Stop();
        }

        private void BtParar_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Para os Engines de Voz e Trilha Sonora
            EngineVoz?.Stop();
            EngineTrilhaSonora?.Stop();
        }
        #endregion Reprodução
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
