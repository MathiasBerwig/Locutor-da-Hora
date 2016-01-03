using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Locutor_da_Hora.Utils;

namespace Locutor_da_Hora.Model
{
    [Serializable]
    public sealed class Locucao : INotifyPropertyChanged
    {
        #region Membros Privados        
        private readonly string uniqueName;
        private string titulo;
        private string texto;
        private bool readOnly;
        #endregion

        #region Construtores Privados
        private Locucao() { }
        #endregion

        #region Construtores Públicos
        public Locucao(string uniqueName = null)
        {
            this.uniqueName = uniqueName ?? Guid.NewGuid().ToString();
        }

        public Locucao(SerializationInfo info, StreamingContext ctxt)
        {
            uniqueName = info.GetString(nameof(UniqueName));
            titulo = info.GetString(nameof(Titulo));
            texto = info.GetString(nameof(Texto));
            readOnly = info.GetBoolean(nameof(ReadOnly));
        }
        #endregion

        #region Membros Públicos
        public string UniqueName => uniqueName;

        public string Titulo
        {
            get { return titulo; }
            set { SetField(ref titulo, value, nameof(Titulo)); }
        }

        public string Texto
        {
            get { return texto; }
            set { SetField(ref texto, value, nameof(Texto)); }
        }

        public bool ReadOnly
        {
            get { return readOnly; }
            set { SetField(ref readOnly, value, nameof(ReadOnly)); }
        }

        public string LocalIcone => GerenciadorLocucoes.DiretorioLocucoes + UniqueName + ".png";

        public string LocalTrilhaSonora => GerenciadorLocucoes.DiretorioLocucoes + UniqueName + ".mp3";

        public ImageSource Icone
        {
            get
            {
                if (!File.Exists(LocalIcone)) return null;

                var icone = new BitmapImage();
                icone.BeginInit();
                icone.CacheOption = BitmapCacheOption.None;
                icone.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                icone.CacheOption = BitmapCacheOption.OnLoad;
                icone.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                icone.UriSource = new Uri(LocalIcone);
                icone.EndInit();
                return icone;
            }
        }
        #endregion

        #region Métodos Privados
        #endregion

        #region Métodos Públicos        
        public void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
        #endregion

        #region INotifyPropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Debug.WriteLine("OnPropertyChanged: " + propertyName);
        }

        private bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        #region ISerializable
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(UniqueName), UniqueName);
            info.AddValue(nameof(Titulo), Titulo);
            info.AddValue(nameof(Texto), Texto);
            info.AddValue(nameof(ReadOnly), ReadOnly);
        }
        #endregion
    }
}
