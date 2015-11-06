using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using NAudio.Wave;
using WPFSoundVisualizationLib;
using Application = System.Windows.Application;

namespace Locutor_da_Hora.Audio
{
    public class NAudioEngine : ISpectrumPlayer, IWaveformPlayer, IDisposable
    {
        #region Membros Privados
        private readonly DispatcherTimer positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private readonly BackgroundWorker waveformGenerateWorker = new BackgroundWorker();
        private readonly int fftDataSize = (int)FFTDataSize.FFT2048;
        private string fileName;
        private bool disposed;
        private bool canPlay;
        private bool canPause;
        private bool canStop;
        private bool canCut;
        private bool canExport;
        private bool isPlaying;
        private bool inChannelTimerUpdate;
        private double channelLength;
        private double channelPosition;
        private bool inChannelSet;
        private WaveOut waveOutDevice;
        private WaveStream activeStream;
        private WaveChannel32 inputStream;
        private SampleAggregator sampleAggregator;
        private SampleAggregator waveformAggregator;
        private string pendingWaveformPath;
        private float[] waveformData;
        private float[] fullLevelData;
        private TimeSpan repeatStart;
        private TimeSpan repeatStop;
        private bool inRepeatSet;
        private bool isGeneratingWaveForm;
        #endregion

        #region Constantes
        private const int WaveformCompressedPointCount = 2000;
        private const int RepeatThreshold = 200;
        #endregion

        #region Construtores Públicos
        public NAudioEngine()
        {
            positionTimer.Interval = TimeSpan.FromMilliseconds(100);
            positionTimer.Tick += positionTimer_Tick;

            waveformGenerateWorker.DoWork += waveformGenerateWorker_DoWork;
            waveformGenerateWorker.RunWorkerCompleted += waveformGenerateWorker_RunWorkerCompleted;
            waveformGenerateWorker.WorkerSupportsCancellation = true;
        }
        #endregion

        #region Membros Públicos
        public WaveStream ActiveStream
        {
            get { return activeStream; }
            protected set { SetField(ref activeStream, value, "ActiveStream"); }
        }

        public bool CanPlay
        {
            get { return canPlay; }
            protected set { SetField(ref canPlay, value, "CanPlay"); }
        }

        public bool CanPause
        {
            get { return canPause; }
            protected set { SetField(ref canPause, value, "CanPause"); }
        }

        public bool CanStop
        {
            get { return canStop; }
            protected set { SetField(ref canStop, value, "CanStop"); }
        }

        public bool CanCut
        {
            get { return canCut; }
            protected set { SetField(ref canCut, value, "CanCut"); }
        }

        public bool CanExport
        {
            get { return canExport; }
            protected set { SetField(ref canExport, value, "CanExport"); }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            protected set
            {
                SetField(ref isPlaying, value, "IsPlaying");
                positionTimer.IsEnabled = value;
            }
        }

        public bool IsGeneratingWaveForm
        {
            get { return isGeneratingWaveForm; }
            protected set { SetField(ref isGeneratingWaveForm, value, "IsGeneratingWaveForm"); }
        }

        public float Volume
        {
            get { return inputStream?.Volume ?? 0; }
            set { if (inputStream != null) inputStream.Volume = value; OnPropertyChanged("Volume"); }
        }

        public string FileName => fileName;
        #endregion        

        #region Métodos Privados
        private void audioOutput_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            ChannelPosition = 0;
            CanStop = false;
            CanPause = false;
            CanPlay = true;
        }

        private void StopAndCloseStream()
        {
            Stop();

            if (waveOutDevice != null)
            {
                waveOutDevice.Dispose();
                waveOutDevice = null;
            }

            if (activeStream != null)
            {
                activeStream.Close();
                activeStream = null;
            }

            if (inputStream != null)
            {
                inputStream.Close();
                inputStream = null;
            }

            CanExport = false;
        }
        #endregion

        #region Métodos Públicos
        public void ClearSelection()
        {
            SelectionBegin = TimeSpan.Zero;
            SelectionEnd = TimeSpan.Zero;
        }

        public void StopAndClear()
        {
            StopAndCloseStream();

            WaveformData = new float[0];
            ChannelLength = 0d;

            CanPlay = false;
            CanPause = false;
            CanStop = false;
            CanExport = false;
        }

        public void Stop()
        {
            if (!CanStop) return;
            waveOutDevice?.Stop();
            IsPlaying = false;
            CanStop = false;
            CanPlay = true;
            CanPause = false;
            ChannelPosition = 0;
        }

        public void Pause()
        {
            if (!IsPlaying || !CanPause) return;
            waveOutDevice?.Pause();
            IsPlaying = false;
            CanPlay = true;
            CanPause = false;
            CanStop = true;
        }

        public void Play()
        {
            if (!CanPlay) return;
            waveOutDevice?.Play();
            IsPlaying = true;
            CanPause = true;
            CanPlay = false;
            CanStop = true;
        }

        public void OpenFile(string filePath)
        {
            SetField(ref fileName, filePath, "FileName");

            ClearSelection();
            StopAndCloseStream();

            try
            {
                waveOutDevice = new WaveOut() { DesiredLatency = 100 };

                ActiveStream = new Mp3FileReader(filePath);
                inputStream = new WaveChannel32(ActiveStream) { PadWithZeroes = false };
                sampleAggregator = new SampleAggregator(4096);
                inputStream.Sample += inputStream_Sample;
                waveOutDevice.Init(inputStream);
                waveOutDevice.PlaybackStopped += audioOutput_PlaybackStopped;
                ChannelLength = inputStream.TotalTime.TotalSeconds;
                GenerateWaveformData(filePath);

                CanPlay = true;
                CanExport = true;
                OnPropertyChanged("Volume");
            }
            catch
            {
                ActiveStream = null;
                CanPlay = false;
                CanExport = false;
            }
        }
        #endregion

        #region Geração do Waveform
        private class WaveformGenerationParams
        {
            public WaveformGenerationParams(int points, string path)
            {
                Points = points;
                Path = path;
            }

            public int Points { get; }
            public string Path { get; }
        }

        private void GenerateWaveformData(string path)
        {
            if (waveformGenerateWorker.IsBusy)
            {
                pendingWaveformPath = path;
                waveformGenerateWorker.CancelAsync();
                return;
            }

            if (!waveformGenerateWorker.IsBusy && WaveformCompressedPointCount != 0)
                waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(WaveformCompressedPointCount, path));
        }

        private void waveformGenerateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled) return;
            if (!waveformGenerateWorker.IsBusy && WaveformCompressedPointCount != 0)
                waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(WaveformCompressedPointCount, pendingWaveformPath));
        }

        private void waveformGenerateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsGeneratingWaveForm = true;

            WaveformGenerationParams waveformParams = e.Argument as WaveformGenerationParams;
            Mp3FileReader waveformMp3Stream = new Mp3FileReader(waveformParams?.Path);
            WaveChannel32 waveformInputStream = new WaveChannel32(waveformMp3Stream);
            waveformInputStream.Sample += waveStream_Sample;

            int frameLength = fftDataSize;
            int frameCount = (int)((double)waveformInputStream.Length / frameLength);
            int waveformLength = frameCount * 2;
            byte[] readBuffer = new byte[frameLength];
            waveformAggregator = new SampleAggregator(frameLength);

            float maxLeftPointLevel = float.MinValue;
            float maxRightPointLevel = float.MinValue;
            int currentPointIndex = 0;
            float[] waveformCompressedPoints = new float[waveformParams.Points];
            List<float> waveformData = new List<float>();
            List<int> waveMaxPointIndexes = new List<int>();

            for (int i = 1; i <= waveformParams.Points; i++)
            {
                waveMaxPointIndexes.Add((int)Math.Round(waveformLength * ((double)i / waveformParams.Points), 0));
            }
            int readCount = 0;
            while (currentPointIndex * 2 < waveformParams.Points)
            {
                waveformInputStream.Read(readBuffer, 0, readBuffer.Length);

                waveformData.Add(waveformAggregator.LeftMaxVolume);
                waveformData.Add(waveformAggregator.RightMaxVolume);

                if (waveformAggregator.LeftMaxVolume > maxLeftPointLevel)
                    maxLeftPointLevel = waveformAggregator.LeftMaxVolume;
                if (waveformAggregator.RightMaxVolume > maxRightPointLevel)
                    maxRightPointLevel = waveformAggregator.RightMaxVolume;

                if (readCount > waveMaxPointIndexes[currentPointIndex])
                {
                    waveformCompressedPoints[(currentPointIndex * 2)] = maxLeftPointLevel;
                    waveformCompressedPoints[(currentPointIndex * 2) + 1] = maxRightPointLevel;
                    maxLeftPointLevel = float.MinValue;
                    maxRightPointLevel = float.MinValue;
                    currentPointIndex++;
                }
                if (readCount % 3000 == 0)
                {
                    float[] clonedData = (float[])waveformCompressedPoints.Clone();
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        WaveformData = clonedData;
                    }));
                }

                if (waveformGenerateWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                readCount++;
            }

            float[] finalClonedData = (float[])waveformCompressedPoints.Clone();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                fullLevelData = waveformData.ToArray();
                WaveformData = finalClonedData;
            }));
            waveformInputStream.Close();
            waveformInputStream.Dispose();
            waveformMp3Stream.Close();
            waveformMp3Stream.Dispose();

            IsGeneratingWaveForm = false;
        }
        #endregion

        #region Eventos
        private void inputStream_Sample(object sender, SampleEventArgs e)
        {
            sampleAggregator.Add(e.Left, e.Right);
            var repeatStartPosition = (long)((SelectionBegin.TotalSeconds / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
            var repeatStopPosition = (long)((SelectionEnd.TotalSeconds / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
            if (((SelectionEnd - SelectionBegin) >= TimeSpan.FromMilliseconds(RepeatThreshold)) && ActiveStream.Position >= repeatStopPosition)
            {
                sampleAggregator.Clear();
                ActiveStream.Position = repeatStartPosition;
            }
        }

        void waveStream_Sample(object sender, SampleEventArgs e)
        {
            waveformAggregator.Add(e.Left, e.Right);
        }

        void positionTimer_Tick(object sender, EventArgs e)
        {
            inChannelTimerUpdate = true;
            if (ActiveStream != null)
            {
                ChannelPosition = (ActiveStream.Position / (double)ActiveStream.Length) * ActiveStream.TotalTime.TotalSeconds;
            }
            inChannelTimerUpdate = false;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing) StopAndCloseStream();
            disposed = true;
        }
        #endregion

        #region ISpectrumPlayer
        public bool GetFFTData(float[] fftDataBuffer)
        {
            sampleAggregator.GetFFTResults(fftDataBuffer);
            return isPlaying;
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            double maxFrequency;
            if (ActiveStream != null)
                maxFrequency = ActiveStream.WaveFormat.SampleRate / 2.0d;
            else
                maxFrequency = 22050; // Assume a default 44.1 kHz sample rate.
            return (int)((frequency / maxFrequency) * 2048);
        }
        #endregion

        #region IWaveformPlayer
        public TimeSpan SelectionBegin
        {
            get { return repeatStart; }
            set
            {
                if (inRepeatSet) return;
                inRepeatSet = true;
                SetField(ref repeatStart, value, "SelectionBegin");
                CanCut = SelectionBegin < SelectionEnd;
                inRepeatSet = false;
            }
        }

        public TimeSpan SelectionEnd
        {
            get { return repeatStop; }
            set
            {
                if (inRepeatSet) return;
                inRepeatSet = true;
                SetField(ref repeatStop, value, "SelectionEnd");
                CanCut = SelectionBegin < SelectionEnd;
                inRepeatSet = false;
            }
        }

        public float[] WaveformData
        {
            get { return waveformData; }
            protected set { SetField(ref waveformData, value, "WaveformData"); }
        }

        public double ChannelLength
        {
            get { return channelLength; }
            protected set { SetField(ref channelLength, value, "ChannelLength"); }
        }

        public double ChannelPosition
        {
            get { return channelPosition; }
            set
            {
                if (inChannelSet) return;
                inChannelSet = true;
                double position = Math.Max(0, Math.Min(value, ChannelLength));
                if (!inChannelTimerUpdate && ActiveStream != null)
                    ActiveStream.Position = (long)((position / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
                SetField(ref channelPosition, value, "ChannelPosition");
                inChannelSet = false;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
        #endregion
    }
}
