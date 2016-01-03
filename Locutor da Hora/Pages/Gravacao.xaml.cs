using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Locutor_da_Hora.Utils;
using Locutor_da_Hora.Audio;
using Locutor_da_Hora.Model;
using Locutor_da_Hora.Windows;

namespace Locutor_da_Hora.Pages
{
    /// <summary>
    /// Lógica de interação para o Gravacao.xaml
    /// </summary>       
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public partial class Gravacao : INotifyPropertyChanged
    {
        #region Membros Privados        
        private Locucao locucao;
        private Stopwatch temporizador;
        private string localGravacao;
        private float ultimoVolume;
        private bool podeVoltar = true;
        private bool podeGravar = true;
        private bool podeInterromperGravacao;
        private bool podeAvancar;
        private static Gravacao instance;
        #endregion

        #region Construtores Privados
        private Gravacao()
        {
            // Inicializa os componentes gráficos
            InitializeComponent();

            // Define o contexto da aplicação
            DataContext = this;
        }
        #endregion

        #region Membros Públicos 
        public static Gravacao Instance => instance ?? (instance = new Gravacao());

        public Locucao Locucao
        {
            get { return locucao; }
            set
            {
                SetField(ref locucao, value, nameof(Locucao));
                var trilhaSonoraExists = File.Exists(locucao.LocalTrilhaSonora);
                // Carrega a Trilha Sonora na Engine de Reprodução
                if (trilhaSonoraExists) EngineTrilhaSonora.OpenFile(locucao.LocalTrilhaSonora);
                // Mostra/Oculta o painel de Volume 
                GrdVolumeReproducao.Visibility = trilhaSonoraExists ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public NAudioEngine EngineTrilhaSonora => Edicao.Instance.EngineTrilhaSonora;

        public bool PodeVoltar
        {
            get { return podeVoltar; }
            set { SetField(ref podeVoltar, value, nameof(PodeVoltar)); }
        }

        public bool PodeGravar
        {
            get { return podeGravar; }
            set { SetField(ref podeGravar, value, nameof(PodeGravar)); }
        }

        public bool PodeInterromperGravacao
        {
            get { return podeInterromperGravacao; }
            set { SetField(ref podeInterromperGravacao, value, nameof(PodeInterromperGravacao)); }
        }

        public bool PodeAvancar
        {
            get { return podeAvancar; }
            set { SetField(ref podeAvancar, value, nameof(PodeAvancar)); }
        }
        #endregion

        #region Métodos Privados

        #endregion

        #region Métodos Públicos
        #endregion

        #region Eventos Bt_Click
        private void BtVoltar_Click(object sender, RoutedEventArgs e)
        {
            // Navega o Frame para a Página Seleção de Locução
            MainWindow.Instance.AbrirPagina(SelecaoLocucao.Instance);
        }

        private void BtIniciarGravacao_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Define o local da gravação
                localGravacao = Environment.CurrentDirectory + Properties.Resources.PastaTemporaria_MP3 + @"\" + Guid.NewGuid() + ".wav";

                // Exclui a gravação (wav) anterior (caso exista)
                File.Delete(localGravacao);

                // Inicia a captura de áudio
                Audio.Captura.Instance.IniciarGravacao(localGravacao);
                // Inicia a reprodução da trilha sonora
                EngineTrilhaSonora.Play();

                // Registra o tempo de gravação
                temporizador = new Stopwatch();
                temporizador.Start();

                PodeGravar = false;
                PodeInterromperGravacao = true;
                PodeVoltar = false;
                PodeAvancar = false;
            }
            catch (Exception exception)
            {
                temporizador = null;
                var erro = Properties.Resources.Exception_IniciarGravacao;
                GoogleAnalyticsTracker.Instance.TrackException(exception.Message, true);
                MessageHelper.ShowError(exception.Message);
            }
        }

        private void BtInterromperGravacao_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Interrompe a gravação e reprodução
                Audio.Captura.Instance.InterromperGravacao();
                EngineTrilhaSonora.Stop();

                // Exclui a gravação (mp3) anterior (caso exista)
                File.Delete(localGravacao.Replace(".wav", ".mp3"));

                // Converte a gravação de wav para mp3
                Audio.Edicao.ConverterWaveParaMp3(localGravacao, localGravacao.Replace(".wav", ".mp3"), 320);

                // Exclui o arquivo WAV
                File.Delete(localGravacao);

                // Salva o novo nome de arquivo
                localGravacao = localGravacao.Replace(".wav", ".mp3");

                // Coleta o tempo da gravação
                temporizador.Stop();
                GoogleAnalyticsTracker.Instance.TrackTime(Contract.Analytics.INTERACOES, Contract.Analytics.Interacoes.GRAVACAO, temporizador.ElapsedMilliseconds, locucao.Titulo);

                // Incrementa o contador de gravações
                Properties.Settings.Default.RecordingCount++;
                Properties.Settings.Default.Save();

                PodeGravar = false;
                PodeInterromperGravacao = false;
                PodeVoltar = false;
                PodeAvancar = true;
            }
            catch (Exception exception)
            {
                temporizador = null;
                var erro = Properties.Resources.Exception_InterromperGravacao;
                GoogleAnalyticsTracker.Instance.TrackException(exception.Message, true);
                MessageHelper.ShowError(erro);
                MessageHelper.ShowError(exception.Message);
            }
        }

        private void BtAvancar_Click(object sender, RoutedEventArgs e)
        {
            Edicao.Instance.EngineVoz.OpenFile(localGravacao);
            Edicao.Instance.EngineAtivo = Edicao.Instance.EngineVoz;
            // Ativa o botão Gravar Novamente
            Edicao.Instance.PodeGravarNovamente = true;

            // Navega o Frame para a Página de Edição
            MainWindow.Instance.AbrirPagina(Edicao.Instance);

            PodeGravar = true;
            PodeInterromperGravacao = false;
            PodeVoltar = true;
            PodeAvancar = false;
        }

        private void BtVolumeReproducao_Click(object sender, RoutedEventArgs e)
        {
            if (EngineTrilhaSonora.Volume > 0)
            {
                ultimoVolume = EngineTrilhaSonora.Volume;
                EngineTrilhaSonora.Volume = 0;
            }
            else
            {
                if (EngineTrilhaSonora.Volume == 0) EngineTrilhaSonora.Volume = ultimoVolume;
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
