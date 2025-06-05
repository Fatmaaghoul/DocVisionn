using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Docvision.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _emailSender;
        private readonly string _password;

        public EmailService(string smtpServer, int smtpPort, string emailSender, string password)
        {
            _smtpServer = smtpServer ?? throw new ArgumentNullException(nameof(smtpServer));
            _smtpPort = smtpPort;
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrEmpty(toEmail))
                throw new ArgumentException("L'adresse e-mail du destinataire ne peut pas être vide.", nameof(toEmail));

            if (string.IsNullOrEmpty(subject))
                throw new ArgumentException("Le sujet de l'e-mail ne peut pas être vide.", nameof(subject));

            if (string.IsNullOrEmpty(body))
                throw new ArgumentException("Le corps de l'e-mail ne peut pas être vide.", nameof(body));

            try
            {
                using (var smtpClient = new SmtpClient(_smtpServer))
                {
                    smtpClient.Port = _smtpPort;
                    smtpClient.Credentials = new NetworkCredential(_emailSender, _password);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSender),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (SmtpException ex)
            {
                // Log l'erreur pour le débogage
                Console.WriteLine($"Erreur SMTP lors de l'envoi de l'e-mail : {ex.Message}");
                Console.WriteLine($"Détails de l'erreur : {ex.InnerException?.Message}");
                throw new ApplicationException("Une erreur s'est produite lors de l'envoi de l'e-mail.", ex);
            }
            catch (Exception ex)
            {
                // Log d'autres erreurs inattendues
                Console.WriteLine($"Erreur inattendue lors de l'envoi de l'e-mail : {ex.Message}");
                throw new ApplicationException("Une erreur inattendue s'est produite lors de l'envoi de l'e-mail.", ex);
            }
        }
    }
}
