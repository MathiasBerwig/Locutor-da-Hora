using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Locutor_da_Hora.Converters;
using Locutor_da_Hora.Model;
using Locutor_da_Hora.Utils;

namespace Locutor_da_Hora.Pages.SubPages
{
    /// <summary>
    /// Interaction logic for EnviarEmail.xaml
    /// </summary>
    public partial class EnviarEmail : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Membros Privados
        private int errors;
        private static EnviarEmail instance;
        private string email;
        private bool emailValido;
        #endregion

        #region Construtores Privados
        private EnviarEmail()
        {
            // Inicializa os componentes gráficos
            InitializeComponent();

            // Define o contexto da aplicação
            DataContext = this;
        }
        #endregion

        #region Membros Públicos
        public static EnviarEmail Instance => instance ?? (instance = new EnviarEmail());

        /// <summary>
        /// Indica se o e-mail informado no TbEmail é válido. Utilizado no Binding da Visibility do BtEnviarEmail.
        /// </summary>
        public bool EmailValido
        {
            get { return emailValido; }
            set { SetField(ref emailValido, value, nameof(EmailValido)); }
        }

        /// <summary>
        /// Endereço de e-mail a ser utilizado no envio. Utilizado no no Binding do TbEmail.
        /// </summary>
        public string Email
        {
            get { return email; }
            set { SetField(ref email, value, nameof(Email)); }
        }

        public MailTemplate MailTemplate;
        #endregion

        #region Métodos Privados
        /// <summary>
        /// Verifica se a string informada é um e-mail válido.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Verdadeiro caso o e-mail seja compatível com o formato "nome@provedor.com".</returns>
        private static bool ChecarEmail(string email)
        {
            return Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        /// <summary>
        /// Remove o HTML contido entre as tags NewsletterInfo. 
        /// </summary>
        public static string RemoveNewsletterSignature(string source)
        {
            return Regex.Replace(source, @"<NewsletterInfo>(.|\n)*?</NewsletterInfo>", string.Empty);
        }

        // TODO: Aprimorar método
        /// <summary>
        /// Realiza uma requisição POST para o servidor especificado no MailTemplate, informando os parâmetros
        /// padrão para assinar uma lista de e-mails.
        /// </summary>
        /// <param name="email">E-mail do Usuário.</param>
        /// <param name="mailTemplate">Template com as informações de conexão.</param>
        private void AssinarNewsletter(string email, MailTemplate mailTemplate)
        {
            ServicePointManager.Expect100Continue = false;

            // TODO: Avaliar possibilidades de reuso. O método GoogleAnalyticsTracker.PostData(Hashtable) utiliza código semelhante.
            using (WebClient client = new WebClient())
            {

                client.UploadValues(mailTemplate.NewsletterUrl, new NameValueCollection()
                {
                    {"form_id", mailTemplate.NewsletterFormId},
                    {"action", "save"},
                    {"controller", "subscribers"},
                    {"wysija-page", "1"},
                    {"wysija[user_list][list_ids]", mailTemplate.NewsletterListId},
                    {"wysija[user][email]", email}
                });

                // TODO: Tratamento de erros
            }
        }

        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                errors++;
            else
                errors--;
            EmailValido = errors == 0;
        }
        #endregion

        #region Métodos Públicos
        public void CarregarBanner(string localArquivo)
        {
            ImgBanner.Source = new BitmapImage(new Uri(localArquivo));
        }
        #endregion

        #region Eventos
        private void Tb_KeyDown(object sender, KeyEventArgs e)
        {
            // Verifica se existem erros e a tecla pressionada é Enter, então simula um click no BtEnviarEmail
            if (errors == 0 && e.Key == Key.Enter) BtEnviarEmail_Click(null, null);
        }

        // TODO: Aprimorar método
        private void BtEnviarEmail_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Interrompe a reprodução
            EditarAudio.Instance.EngineTrilhaSonora.Stop();
            EditarAudio.Instance.EngineVoz.Stop();

            // Obtém os valores de variáveis na thread principal
            string arquivoTemporario = Environment.CurrentDirectory + Properties.Resources.PastaTemporaria_MP3 + @"\Gravação Campus Future.mp3";
            string destinatario = TbEmail.Text;
            string arquivoTrilha = EditarAudio.Instance.EngineTrilhaSonora.FileName;
            string arquivoVoz = EditarAudio.Instance.EngineVoz.FileName;
            float volumeTrilha = (float)EditarAudio.Instance.SliderVolumeTrilha.Value;
            float volumeVoz = (float)EditarAudio.Instance.SliderVolumeVoz.Value;
            bool assinarNewsletter = CbAssinarNewsletter.IsChecked.Value;

            // Prepara o e-mail com as informações do usuário
            TagsToValuesTextConverter converter = new TagsToValuesTextConverter();
            string htmlBody = string.Join("", File.ReadAllLines(MailTemplate.HtmlFile));
            htmlBody = converter.Convert(htmlBody, null, Identificacao.Instance.Usuario, null).ToString();

            // Remove a mensagem de assinatura de newsletter caso o usuário não tenha marcado o CbAssinarNewsletter
            if (!assinarNewsletter)
            {
                htmlBody = RemoveNewsletterSignature(htmlBody);
            }

            // Inicia o envio do e-mail em uma thread auxiliar
            Task.Factory.StartNew(() =>
            {
                Edicao.Instance.Processando = true;

                try
                {
                    // Verifica se a voz e a trilha sonora estão disponíveis
                    if (EditarAudio.Instance.EngineVoz.CanExport && EditarAudio.Instance.EngineTrilhaSonora.CanExport)
                    {
                        // Mescla as trilhas em uma nova stream
                        MemoryStream novaStream = Audio.Edicao.MesclarMp3(arquivoTrilha, volumeTrilha, arquivoVoz, volumeVoz);
                        // Exporta voz e trilha sonora
                        Audio.Edicao.ExportarMp3(novaStream, arquivoTemporario, true);
                    }
                    else
                    {
                        // Exportar apenas Voz
                        File.Copy(EditarAudio.Instance.EngineVoz.FileName, arquivoTemporario, true);
                    }

                    // Envia o e-mail
                    MailHelper.sendEmail(MailTemplate, htmlBody, destinatario, arquivoTemporario);

                    // Assina o newsletter com o e-mail do usuário
                    if (assinarNewsletter)
                    {
                        AssinarNewsletter(destinatario, MailTemplate);
                    }
                }
                catch (Exception exception)
                {
                    var erro = Properties.Resources.Exception_EnviarEmail;
                    GoogleAnalyticsTracker.Instance.TrackException(erro, true);
                    MessageHelper.ShowError(exception.Message);
                }
            }).ContinueWith(t1 =>
            {
                Edicao.Instance.Processando = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
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

        #region IDataErrorInfo
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {

                    case nameof(Email):
                        return string.IsNullOrWhiteSpace(Email) || !ChecarEmail(Email) ? Properties.Resources.Validation_Email : null;
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

        private void BtCancelar_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TbEmail.Text = "";
            Edicao.Instance.FramePrincipal.Navigate(EditarAudio.Instance);
        }
    }
}
