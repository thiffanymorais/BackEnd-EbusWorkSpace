using MassTransit;
using JCA.WorkSpace.Domain.Messages;
using JCA.WorkSpace.Infrastructure.Data.Email;
using Microsoft.Extensions.Logging;

namespace JCA.WorkSpace.Service.API.Consumers;

/// <summary>
/// Worker que fica "escutando" a fila do RabbitMQ em segundo plano.
/// Quando chega uma mensagem SendWelcomeEmailMessage, ele chama
/// o EmailService para montar e enviar o e-mail de boas-vindas.
/// </summary>
public class WelcomeEmailConsumer : IConsumer<SendWelcomeEmailMessage>
{
    private readonly EmailService _emailService;
    private readonly ILogger<WelcomeEmailConsumer> _logger;

    public WelcomeEmailConsumer(EmailService emailService, ILogger<WelcomeEmailConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendWelcomeEmailMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER] 📨 Processando e-mail de boas-vindas para: {Name} <{Email}>",
            msg.RecipientName,
            msg.RecipientEmail);

        await _emailService.SendWelcomeEmailAsync(
            msg.RecipientName,
            msg.RecipientEmail,
            msg.Role,
            msg.Sector);
    }
}
