using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JCA.WorkSpace.Infrastructure.Data.Email;

/// <summary>
/// Serviço responsável por montar o template HTML e enviar e-mails via SMTP.
/// Em desenvolvimento aponta para o Mailpit (localhost:1025) — nenhum e-mail real é enviado.
/// Em produção, basta alterar as configurações no appsettings.json.
/// </summary>
public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(
        string recipientName,
        string recipientEmail,
        string role,
        string? sector)
    {
        var settings = _configuration.GetSection("EmailSettings");
        var fromAddress = settings["FromAddress"] ?? "nao-responda@ebusworkspace.com.br";
        var fromName = settings["FromName"] ?? "EbusWorkSpace";
        var plataformaUrl = settings["PlataformaUrl"] ?? "http://localhost:5173";
        var smtpHost = settings["SmtpHost"] ?? "localhost";
        var smtpPort = int.Parse(settings["SmtpPort"] ?? "1025");

        var htmlBody = $@"
        <!DOCTYPE html>
        <html lang='pt-BR'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Bem-vindo ao EbusWorkSpace</title>
        </head>
        <body style='margin:0;padding:0;background-color:#f4f6f9;font-family:Segoe UI,Arial,sans-serif;'>
          <div style='padding:40px 0;'>
            <div style='background:#ffffff;margin:0 auto;max-width:600px;border-radius:12px;overflow:hidden;border:1px solid #e9ecef;box-shadow:0 4px 12px rgba(0,0,0,0.06);'>

              <!-- Cabeçalho -->
              <div style='background:linear-gradient(135deg,#0d3b66 0%,#001e3d 100%);padding:32px 40px;text-align:center;'>
                <h1 style='color:#ffffff;margin:0;font-size:24px;font-weight:700;'>🏢 EbusWorkSpace</h1>
                <p style='color:#b0c4de;margin:6px 0 0 0;font-size:14px;'>Gestão Inteligente de Espaços e Mesas</p>
              </div>

              <!-- Conteúdo -->
              <div style='padding:36px 40px;color:#2d3748;line-height:1.6;'>
                <h2 style='color:#1a202c;font-size:20px;'>Olá, {recipientName}! 👋</h2>
                <p>Seu acesso ao <strong>EbusWorkSpace</strong> está ativo e configurado. Agora você pode reservar postos de trabalho e salas de reunião com agilidade.</p>

                <!-- Box de dados do usuário -->
                <div style='background-color:#f8fafc;border-left:4px solid #0d3b66;border-radius:6px;padding:18px;margin:24px 0;'>
                  <div style='font-size:15px;font-weight:700;color:#0d3b66;margin-bottom:10px;'>📋 Seus Dados de Acesso:</div>
                  <div style='margin-bottom:6px;'><strong>E-mail:</strong> {recipientEmail}</div>
                  <div style='margin-bottom:6px;'><strong>Setor:</strong> {sector ?? "Não informado"}</div>
                  <div><strong>Perfil:</strong> {role}</div>
                </div>

                <!-- Alerta de No-Show -->
                <div style='background-color:#fffaf0;border:1px solid #feebc8;border-radius:8px;padding:14px 18px;margin:20px 0;font-size:13px;color:#7b341e;'>
                  <strong>⚠️ Regras Importantes de Utilização:</strong>
                  <ul style='margin:8px 0 0 0;padding-left:20px;'>
                    <li><strong>Check-in Obrigatório:</strong> Lembre-se de realizar o check-in na plataforma ao chegar no escritório.</li>
                    <li><strong>No-Show Automático:</strong> Reservas sem confirmação de presença até as <strong>10:30</strong> são canceladas automaticamente para liberação do espaço.</li>
                  </ul>
                </div>

                <!-- Botão de acesso -->
                <div style='text-align:center;margin:32px 0 16px;'>
                  <a href='{plataformaUrl}' style='display:inline-block;background-color:#0d3b66;color:#ffffff;padding:12px 28px;border-radius:6px;text-decoration:none;font-weight:600;font-size:14px;'>Acessar o EbusWorkSpace</a>
                </div>
              </div>

              <!-- Rodapé -->
              <div style='background-color:#f8fafc;padding:24px 40px;text-align:center;font-size:12px;color:#a0aec0;border-top:1px solid #edf2f7;'>
                <p style='margin:0;'>© 2026 Grupo JCA — Mensagem gerada automaticamente pelo sistema. Não responda este e-mail.</p>
              </div>

            </div>
          </div>
        </body>
        </html>";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(recipientName, recipientEmail));
        message.Subject = "🏢 Bem-vindo ao EbusWorkSpace!";
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();

        // Em desenvolvimento: sem SSL/TLS (compatível com Mailpit)
        // Em produção: trocar para SecureSocketOptions.StartTls
        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.None);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("[EMAIL] ✅ E-mail de boas-vindas enviado para: {Email}", recipientEmail);
    }
}
