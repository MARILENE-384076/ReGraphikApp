using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService
{
    /// <summary>
    /// Configure os dados do seu servidor de e-mail aqui
    /// </summary>
    private const string SmtpHost = "smtp.gmail.com"; /// Servidor SMTP do Gmail
    private const int SmtpPort = 587; /// Porta TLS do Gmail
    private const string EmailRemetente = "reghaphiktcc@gmail.com"; /// E-mail do remetente (deve ser o mesmo da conta do Gmail)
    private const string SenhaRemetente = "mqfydsuvpuswekiw"; /// Senha de aplicativo do Gmail (não use a senha normal da conta)

    /// <summary>
    /// Envia um token de acesso por e-mail ao destinatário.
    /// </summary>
    /// <param name="emailDestinatario">O e-mail do destinatário.</param>
    /// <param name="token">O token de acesso a ser enviado.</param>
    /// <returns></returns>
    public async Task EnviarTokenPorEmailAsync(string emailDestinatario, string token)
    {
        try
        {
            using (var mensagem = new MailMessage())
            {
                mensagem.From = new MailAddress(EmailRemetente, "Administração do Sistema");
                mensagem.To.Add(new MailAddress(emailDestinatario));
                mensagem.Subject = "Seu Convite de Acesso ao Sistema";

                /// Corpo do e-mail em HTML para ficar profissional
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

                    /// Envia de forma assíncrona para não travar a tela (UI)
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

    /// <summary>
    /// Envia um link de redefinição de senha para o destinatário.
    /// </summary>
    /// <param name="emailDestinatario"></param>
    /// <param name="linkRedefinicao"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task EnviarLinkRecuperacaoAsync(string emailDestinatario, string linkRedefinicao)
    {
        try
        {
            /// Corpo do e-mail em HTML para ficar profissional
            using (var mensagem = new MailMessage())
            {
                mensagem.From = new MailAddress(EmailRemetente, "Suporte ReGraphik");
                mensagem.To.Add(new MailAddress(emailDestinatario));
                mensagem.Subject = "Redefinição de Senha - ReGraphik";

                /// Corpo do e-mail em HTML para ficar profissional
                mensagem.IsBodyHtml = true;
                mensagem.Body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #E2E8F0; border-radius: 12px; padding: 24px;'>
                    <h2 style='color: #3B82F6; margin-top: 0;'>Recuperação de Senha</h2>
                    <p style='color: #334155; font-size: 14px; line-height: 1.5;'>
                        Recebemos uma solicitação para redefinir a senha da sua conta no sistema <strong>ReGraphik</strong>. 
                        Para criar uma nova senha, clique no botão azul abaixo:
                    </p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{linkRedefinicao}' target='_blank' style='background-color: #3B82F6; color: white; padding: 12px 24px; text-decoration: none; font-size: 15px; font-weight: bold; border-radius: 8px; display: inline-block;'>
                            Redefinir Minha Senha
                        </a>
                    </div>
                    <p style='color: #64748B; font-size: 12px; line-height: 1.4;'>
                        Se você não solicitou essa alteração, pode ignorar este e-mail com segurança. Sua senha atual continuará funcionando.
                    </p>
                    <hr style='border: 0; border-top: 1px solid #E2E8F0; margin: 20px 0;' />
                    <p style='color: #94A3B8; font-size: 11px;'>
                        Caso o botão não funcione, copie e cole o link abaixo no seu navegador:<br/>
                        <a href='{linkRedefinicao}' style='color: #3B82F6; word-break: break-all;'>{linkRedefinicao}</a>
                    </p>
                </div>";

                using (var smtpClient = new SmtpClient(SmtpHost, SmtpPort))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(EmailRemetente, SenhaRemetente);

                    await smtpClient.SendMailAsync(mensagem);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Falha ao enviar o e-mail de recuperação: " + ex.Message);
        }
    }
}