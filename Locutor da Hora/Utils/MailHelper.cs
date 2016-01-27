using System.Net;
using System.Net.Mail;
using Locutor_da_Hora.Model;

namespace Locutor_da_Hora.Utils
{
    /// <summary>
    /// Classe auxiliar responsável por auxiliar no envio de e-mails com corpo HTML e anexo.
    /// </summary>
    public sealed class MailHelper
    {
        public static void sendEmail(MailTemplate mailTemplate, string htmlBody, string emailTo, string attachment)
        {
            using (MailMessage mail = new MailMessage())
            {
                // Preenche as informações da mensagem
                mail.From = new MailAddress(mailTemplate.EmailFrom);
                mail.To.Add(emailTo);
                mail.Subject = mailTemplate.Subject;
                mail.Body = htmlBody;
                mail.IsBodyHtml = true;

                // Verifica se o arquivo de anexo existe, e então adiciona-o ao e-mail
                if (!string.IsNullOrWhiteSpace(attachment) && System.IO.File.Exists(attachment))
                {
                    mail.Attachments.Add(new Attachment(attachment));
                }

                using (SmtpClient smtp = new SmtpClient(mailTemplate.SmtpServer, mailTemplate.PortNumber))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(mailTemplate.Username, mailTemplate.Password);
                    smtp.EnableSsl = mailTemplate.EnableSsl;
                    smtp.Timeout = 10000;
                    smtp.Send(mail);
                }
            }
        }
    }
}
