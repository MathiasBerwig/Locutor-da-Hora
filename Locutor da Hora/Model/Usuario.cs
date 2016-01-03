using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Locutor_da_Hora.Model
{
    public class Usuario : IDataErrorInfo, INotifyPropertyChanged
    {
        #region Membros Privados
        private string nome;
        private string email;
        private string radio;
        private string cidade;
        private string uf;
        #endregion

        #region Construtores Públicos
        public Usuario() { }

        public Usuario(string nome, string email, string radio, string cidade, string uf)
        {
            this.nome = nome;
            this.email = email;
            this.radio = radio;
            this.cidade = cidade;
            this.uf = uf;
        }
        #endregion

        #region Membros Públicos
        public string Nome
        {
            get { return nome; }
            set { SetField(ref nome, value, nameof(Nome)); }
        }

        public string Email
        {
            get { return email; }
            set { SetField(ref email, value, nameof(Email)); }
        }

        public string Radio
        {
            get { return radio; }
            set { SetField(ref radio, value, nameof(Radio)); }
        }

        public string Cidade
        {
            get { return cidade; }
            set { SetField(ref cidade, value, nameof(Cidade)); }
        }

        public string Uf
        {
            get { return uf; }
            set { SetField(ref uf, value, nameof(Uf)); }
        }
        #endregion

        #region Métodos Auxiliares
        /// <summary>
        /// Verifica se a string informada é um e-mail válido.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Verdadeiro caso o e-mail seja compatível com o formato "nome@provedor.com".</returns>
        private static bool ChecarEmail(string email)
        {
            return Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
        #endregion

        #region IDataErrorInfo
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Nome):
                        return string.IsNullOrWhiteSpace(Nome) ? Properties.Resources.Validation_Nome : null;
                    case nameof(Email):
                        return string.IsNullOrWhiteSpace(Email) || !ChecarEmail(Email) ? Properties.Resources.Validation_Email : null;
                    case nameof(Radio):
                        return string.IsNullOrEmpty(Radio)
                            ? Properties.Resources.Validation_Radio
                            : null;
                    case nameof(Cidade):
                        return string.IsNullOrEmpty(Cidade) ? Properties.Resources.Validation_Cidade : null;
                    case nameof(Uf):
                        return string.IsNullOrEmpty(Uf) ? Properties.Resources.Validation_UF : null;
                    default:
                        return null;
                }
            }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region INotifyPropertyChanged        
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
    }
}
