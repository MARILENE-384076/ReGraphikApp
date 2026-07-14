using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService
{
    // Configure os dados do seu servidor de e-mail aqui
    private const string SmtpHost = "smtp.gmail.com"; // ex: smtp.office365.com ou smtp.gmail.com
    private const int SmtpPort = 587; // Geralmente 587 para TLS
    private const string EmailRemetente = "reghaphiktcc@gmail.com";
    private const string SenhaRemetente = "mqfydsuvpuswekiw";

    public async Task EnviarTokenPorEmailAsync(string emailDestinatario, string token)
    {
        try
        {
            using (var mensagem = new MailMessage())
            {
                mensagem.From = new MailAddress(EmailRemetente, "Administração do Sistema");
                mensagem.To.Add(new MailAddress(emailDestinatario));
                mensagem.Subject = "Seu Convite de Acesso ao Sistema";

                // Corpo do e-mail em HTML para ficar profissional
                mensagem.IsBodyHtml = true;
                mensagem.Body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #E2E8F0; border-radius: 12px; padding: 24px;'>
                    <h2 style='color: #1D4ED8; margin-top: 0;'>Seja bem-vindo!</h2>
                    <p style='color: #334155; font-size: 14px; line-height: 1.5;'>
                        Você foi convidado a fazer parte do sistema. Use o token de acesso abaixo para realizar o seu cadastro.
                    </p>
                    <div style='background-color: #F8FAFC; border: 1px dashed #1D4ED8; border-radius: 8px; padding: 16px; margin: 24px 0; text-align: center;'>
                        <span style='font-family: monospace; font-size: 24px; font-weight: bold; color: #1D4ED8; letter-spacing: 2px;'>{token}</span>
                    </div>
                    <p style='color: #64748B; font-size: 12px;'>
                        * Este token é de uso único e expira automaticamente em 30 minutos.
                    </p>
                </div>";

                using (var smtpClient = new SmtpClient(SmtpHost, SmtpPort))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(EmailRemetente, SenhaRemetente);

                    // Envia de forma assíncrona para não travar a tela (UI)
                    await smtpClient.SendMailAsync(mensagem);
                }
            }
        }
        catch (Exception ex)
        {
            // Propaga o erro para ser capturado no try/catch da sua ViewModel
            throw new Exception("Falha ao disparar o e-mail: " + ex.Message);
        }
    }
}