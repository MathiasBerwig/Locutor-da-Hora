using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using Locutor_da_Hora.Model;

namespace Locutor_da_Hora.Utils
{
    class GerenciadorLocucoes
    {
        #region Membros Privados
        private static GerenciadorLocucoes instance;
        private static readonly string diretorioLocucoes = AppDomain.CurrentDomain.BaseDirectory + @"Locuções\";
        private static readonly string arquivoLocucoes = Properties.Resources.Arquivo_Locucoes;
        #endregion

        #region Membros Públicos
        public static GerenciadorLocucoes Instance => instance ?? (instance = new GerenciadorLocucoes());

        public ObservableCollection<Locucao> Locucoes { get; private set; }

        public static string DiretorioLocucoes => diretorioLocucoes;

        /// <summary>
        /// Define o nome único para ser usado para identificar o botão Adicionar Locução.
        /// </summary>
        public static readonly string ADICIONAR_LOCUCAO = "adicionar_locucao";
        #endregion

        #region Métodos Privados

        #endregion

        #region Métodos Públicos
        public void CarregarLocucoes()
        {
            try
            {
                Locucoes = SerializationHelper.ReadFromBinaryFile<ObservableCollection<Locucao>>(diretorioLocucoes + arquivoLocucoes);
            }
            catch (Exception)
            {
                Locucoes = new ObservableCollection<Locucao> { new Locucao(ADICIONAR_LOCUCAO) {
                    Titulo = Properties.Resources.Locucao_Adicionar,
                    ReadOnly = true } };
            }
        }

        public void SalvarLocucoes()
        {
            try
            {
                Directory.CreateDirectory(diretorioLocucoes);
                SerializationHelper.WriteToBinaryFile(diretorioLocucoes + arquivoLocucoes, Locucoes);
            }
            catch (Exception exception)
            {
                MessageHelper.ShowError(Properties.Resources.Exception_SalvarLocucoes);
                GoogleAnalyticsTracker.Instance.TrackException(exception.Message, true);
            }
        }
        #endregion
    }
}
