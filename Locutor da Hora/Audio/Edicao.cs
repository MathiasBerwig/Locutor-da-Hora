using System;
using System.IO;
using System.Linq;
using NAudio.Lame;
using NAudio.Wave;

namespace Locutor_da_Hora.Audio
{
    public class Edicao
    {        
        #region Conversão de Formatos
        /// <summary>
        /// Realiza a conversão de arquivos WAV para MP3 utilizando o codec LAME.
        /// </summary>
        /// <param name="waveFileName">Arquivo Wave de origem. Exemplo: "C:\Origem.wav"</param>
        /// <param name="mp3FileName">Arquivo MP3 de destino. Exemplo: "C:\Destino.mp3"</param>
        /// <param name="bitRate">Bitragem do arquivo de destino.</param>
        public static void ConverterWaveParaMp3(string waveFileName, string mp3FileName, int bitRate = 128)
        {
            using (var reader = new WaveFileReader(waveFileName))
            using (var writer = new LameMP3FileWriter(mp3FileName, reader.WaveFormat, bitRate))
                reader.CopyTo(writer);
        }

        /// <summary>
        /// Realiza a conversão de arquivos MP3 para WAV utilizando o codec LAME.
        /// </summary>
        /// <param name="mp3FileName">Arquivo MP3 de origem. Exemplo: "C:\Origem.mp3"</param>
        /// <param name="waveFileName">Arquivo Wave de destino. Exemplo: "C:\Destino.wav"</param>        
        public static void ConverterMp3ParaWave(string mp3FileName, string waveFileName)
        {
            using (var reader = new Mp3FileReader(mp3FileName))
            using (var writer = new WaveFileWriter(waveFileName, reader.WaveFormat))
                reader.CopyTo(writer);
        }

        /// <summary>
        /// Converte um WaveStream para MP3Stream.
        /// </summary>
        /// <param name="wavFile"/>
        /// <returns></returns>
        public static MemoryStream ConverterWaveStreamParaMp3Stream(Wave32To16Stream wavFile)
        {
            var retMs = new MemoryStream();
            using (var wtr = new LameMP3FileWriter(retMs, wavFile.WaveFormat, 128))
            {
                wavFile.CopyTo(wtr);
                return retMs;
            }
        }
        #endregion

        #region Edição
        /// <summary>
        /// Transforma o período selecionado em um novo MP3 utilizando o codec LAME.
        /// </summary>
        /// <param name="inputFile">Arquivo de Entrada.</param>                
        /// <param name="inicio">Início da Seleção.</param>
        /// <param name="fim">Fim da Seleção.</param>        
        public static MemoryStream CortarMp3(string inputFile, TimeSpan? inicio, TimeSpan? fim)
        {
            MemoryStream writer = new MemoryStream();
            using (var reader = new Mp3FileReader(inputFile))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                    if (reader.CurrentTime >= inicio || !inicio.HasValue)
                    {
                        if (reader.CurrentTime <= fim || !fim.HasValue)
                            writer.Write(frame.RawData, 0, frame.RawData.Length);
                        else break;
                    }
            }
            return writer;
        }

        /// <summary>
        /// Remove o período selecionado e salva em um novo MP3 utilizando o codec LAME.
        /// </summary>
        /// <param name="inputStream">Arquivo de Entrada.</param>        
        /// <param name="inicio">Início da Seleção.</param>
        /// <param name="fim">Fim da Seleção.</param>
        public static MemoryStream RemoverTrechoMp3(string inputFile, TimeSpan? inicio, TimeSpan? fim)
        {
            MemoryStream writer = new MemoryStream();
            using (var reader = new Mp3FileReader(inputFile))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                    if ((reader.CurrentTime <= inicio || !inicio.HasValue) || (reader.CurrentTime >= fim || !fim.HasValue))
                    {
                        writer.Write(frame.RawData, 0, frame.RawData.Length);
                    }
            }
            return writer;
        }

        /// <summary>
        /// Concatena dois ou mais arquivos MP3 utilizando o codec LAME. 
        /// </summary>
        /// <param name="arquivosEntrada">Array de arquivos MP3.</param>        
        public static MemoryStream ConcatenarMp3(string[] arquivosEntrada)
        {
            MemoryStream output = new MemoryStream();
            foreach (Mp3FileReader reader in arquivosEntrada.Select(file => new Mp3FileReader(file)))
            {
                if ((output.Position == 0) && (reader.Id3v2Tag != null))
                {
                    output.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                }
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    output.Write(frame.RawData, 0, frame.RawData.Length);
                }
            }
            return output;
        }


        /// <summary>
        /// Mescla dois arquivos Mp3, sobrepondo suas faixas.
        /// </summary>                
        public static MemoryStream MesclarMp3(string arquivoTrilhaSonora, float volumeTrilha, string arquivoVoz, float volumeVoz)
        {
            // Lê os arquivos MP3 do disco
            Mp3FileReader mpTrilhaSonora = new Mp3FileReader(arquivoTrilhaSonora);
            Mp3FileReader mpVoz = new Mp3FileReader(arquivoVoz);

            //Decodifica os arquivos MP3
            WaveStream trilhaSonora = WaveFormatConversionStream.CreatePcmStream(mpTrilhaSonora);
            WaveStream voz = WaveFormatConversionStream.CreatePcmStream(mpVoz);

            var mixer = new WaveMixerStream32 { AutoStop = true };

            // TODO: Adicionar funcionalidade de Offset
            //var vozOffset = trilhaSonora.TotalTime;
            //var vozOffsetted = new WaveOffsetStream(voz, TimeSpan.FromSeconds(10), TimeSpan.Zero, voz.TotalTime.Subtract(TimeSpan.FromSeconds(30)));

            var trilhaSonora32 = new WaveChannel32(trilhaSonora) { PadWithZeroes = false, Volume = volumeTrilha };
            mixer.AddInputStream(trilhaSonora32);

            var voz32 = new WaveChannel32(voz) { PadWithZeroes = false, Volume = volumeVoz };
            mixer.AddInputStream(voz32);

            var wave32 = new Wave32To16Stream(mixer);
            // Codifica o Wave para MP3
            return ConverterWaveStreamParaMp3Stream(wave32);
        }
        #endregion

        #region Exportação
        public static bool ExportarMp3(MemoryStream mp3Stream, String path, bool disposeStream)
        {
            if (mp3Stream == null) return false;
            try
            {
                File.Delete(path);
                mp3Stream.Seek(0, SeekOrigin.Begin);
                using (var writer = File.Create(path))
                using (var reader = new Mp3FileReader(mp3Stream))
                {
                    Mp3Frame frame;
                    while ((frame = reader.ReadNextFrame()) != null)
                        writer.Write(frame.RawData, 0, frame.RawData.Length);
                }
                if (disposeStream) mp3Stream.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}