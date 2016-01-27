using System;

namespace Locutor_da_Hora.Model
{
    /// <summary>
    /// Template com informações de e-mail. Modelo utilizado para serialização, posteriormente processado pela classe Utils/MailHelper.
    /// </summary>
    [Serializable]
    public sealed class MailTemplate
    {
        #region Construtores Públicos
        public MailTemplate()
        {
        }
        #endregion

        #region Membros Públicos
        public string SmtpServer;
        public int PortNumber;
        public bool EnableSsl;
        public string EmailFrom;
        public string Username;
        public string Password;
        public string Subject;
        public string HtmlFile;
        public string BannerImagePath;
        public string NewsletterUrl;
        public string NewsletterFormId;
        public string NewsletterListId;
        #endregion
    }
}
