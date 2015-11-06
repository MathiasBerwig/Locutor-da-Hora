using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Locutor_da_Hora.Audio
{
    class Captura : INotifyPropertyChanged
    {        
        
        #region Membros Privados
        private static Captura instance;
        private WaveIn waveSource;
        private WaveFileWriter waveFile;        
        /// <summary>
        /// Quantidade de dispositivos de entrada (gravação) disponíveis no sistema.
        /// </summary>
        private int waveInDevices;
        private int selectedWaveInDevice = 0;
        private List<string> waveInDevicesName;

        #endregion

        #region Construtores Privados
        /// <summary>
        /// Construtor Default. Privado para forçar o instanciamento.
        /// </summary>
        private Captura()
        {            
            AtualizarDispositivosGravacao();
        }
        #endregion

        #region Membros Públicos
        public static Captura Instance => instance ?? (instance = new Captura());

        /// <summary>
        /// Indice do dispositivo de entrada (gravação) selecionado.
        /// </summary>
        public int SelectedWaveInDevice
        {
            get { return selectedWaveInDevice; }
            set { selectedWaveInDevice = value; NotifyPropertyChanged("SelectedWaveInDevice"); }
        }

        public List<string> WaveInDevicesName
        {
            get { return waveInDevicesName ?? (waveInDevicesName = new List<string>()); }
        }        
        #endregion        

        #region Métodos Privados
        void waveSource_DataAvailable_write(object sender, WaveInEventArgs e)
        {
            if (waveFile == null) return;
            waveFile.Write(e.Buffer, 0, e.BytesRecorded);
            waveFile.Flush();
        }        
        #endregion

        #region Métodos Públicos
        /// <summary>
        /// Inicia a captura de som do microfone
        /// </summary>        
        public void IniciarGravacao(string localArquivo)
        {
            waveSource = new WaveIn {DeviceNumber = SelectedWaveInDevice, WaveFormat = new WaveFormat(44100, 1)};
            waveSource.DataAvailable += waveSource_DataAvailable_write;

            if (File.Exists(localArquivo))
                File.Delete(localArquivo);

            var directory = Path.GetDirectoryName(localArquivo);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            waveFile = new WaveFileWriter(localArquivo, waveSource.WaveFormat);
            waveSource.StartRecording();
        }

        /// <summary>
        /// Interrompe a captura de som do microfone.
        /// </summary>
        public void InterromperGravacao()
        {            
            if (waveSource != null)
            {
                waveSource.StopRecording();
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
        }

        /// <summary>
        /// Atualiza a lista <code>waveInDevicesName</code> com o nome dos dispositivos de gravação disponíveis.
        /// </summary>
        public int AtualizarDispositivosGravacao()
        {
            waveInDevices = WaveIn.DeviceCount;

            WaveInDevicesName.Clear();

            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);

                WaveInDevicesName.Add(deviceInfo.ProductName);                
            }

            NotifyPropertyChanged("WaveInDevicesName");

            return WaveInDevicesName.Count;
        }
        #endregion        

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion
    }
}
