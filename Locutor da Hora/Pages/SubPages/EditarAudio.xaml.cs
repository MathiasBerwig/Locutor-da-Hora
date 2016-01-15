using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Locutor_da_Hora.Audio;
using Locutor_da_Hora.Utils;
using Microsoft.Win32;

namespace Locutor_da_Hora.Pages.SubPages
{
    /// <summary>
    /// Interaction logic for EditarAudio.xaml
    /// </summary>
    public partial class EditarAudio : INotifyPropertyChanged
    {
        #region Membros Privados
        private NAudioEngine engineVoz;
        private NAudioEngine engineTrilhaSonora;
        private NAudioEngine engineAtivo;
        private static EditarAudio instance;
        #endregion

        #region Construtores Públicos
        public EditarAudio()
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
        public static EditarAudio Instance => instance ?? (instance = new EditarAudio());

        public NAudioEngine EngineVoz => engineVoz ?? (engineVoz = new NAudioEngine());

        public NAudioEngine EngineTrilhaSonora => engineTrilhaSonora ?? (engineTrilhaSonora = new NAudioEngine());

        public NAudioEngine EngineAtivo
        {
            get { return engineAtivo; }
            set { SetField(ref engineAtivo, value, nameof(EngineAtivo)); }
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
        #region Waveform
        private void Waveform_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DefinirEngineAtivo(sender);
        }

        private void Waveform_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Reproduz o Engine Ativo
            EngineAtivo?.Play();
        }

        #region Menu de Contexto
        private void Waveform_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            DefinirEngineAtivo(sender);
        }

        private void MiCarregar_Click(object sender, RoutedEventArgs e)
        {

            // Abre o diálogo de seleção do arquivo
            var dialog = new OpenFileDialog
            {
                Filter = Properties.Resources.Dialog_Filter_MP3,
                Title = Properties.Resources.Dialog_SelecionarAudio,
                Multiselect = false
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
                Edicao.Instance.Processando = true;

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
            }).ContinueWith(t1 =>
            {
                // Carrega a nova Stream a partir da thread principal (UI)
                EngineAtivo.OpenFile(novoArquivo);

                Edicao.Instance.Processando = false;
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
                Edicao.Instance.Processando = true;

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

            }).ContinueWith(t1 =>
            {
                // Carrega a nova Stream a partir da thread principal (UI)
                EngineAtivo.OpenFile(novoArquivo);

                Edicao.Instance.Processando = false;
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
                Edicao.Instance.Processando = true;

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

                Edicao.Instance.Processando = false;
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

        private void BtReproduzir_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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

        private void BtPausar_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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

        private void BtParar_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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
