using MassTransit;
using JCA.WorkSpace.Domain.Messages;
using JCA.WorkSpace.Infrastructure.Data.Email;

namespace JCA.WorkSpace.Service.API.Consumers;

public class AccessRequestPendingConsumer : IConsumer<AccessRequestPendingMessage>
{
    private readonly EmailService _emailService;
    private readonly ILogger<AccessRequestPendingConsumer> _logger;

    public AccessRequestPendingConsumer(EmailService emailService, ILogger<AccessRequestPendingConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AccessRequestPendingMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER] 📨 Aviso de solicitação de {Profile} feita por {Requester} para o administrador {Admin}",
            msg.RequestedProfile,
            msg.RequesterEmail,
            msg.AdminEmail);

        await _emailService.SendAccessRequestPendingEmailAsync(
            msg.AdminName,
            msg.AdminEmail,
            msg.RequesterName,
            msg.RequesterEmail,
            msg.RequestedProfile);
    }
}

public class AccessRequestApprovedConsumer : IConsumer<AccessRequestApprovedMessage>
{
    private readonly EmailService _emailService;
    private readonly ILogger<AccessRequestApprovedConsumer> _logger;

    public AccessRequestApprovedConsumer(EmailService emailService, ILogger<AccessRequestApprovedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AccessRequestApprovedMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER] 📨 Perfil {Profile} aprovado para {Email}",
            msg.Profile,
            msg.RecipientEmail);

        await _emailService.SendAccessRequestApprovedEmailAsync(
            msg.RecipientName,
            msg.RecipientEmail,
            msg.Profile);
    }
}

public class AccessRequestRejectedConsumer : IConsumer<AccessRequestRejectedMessage>
{
    private readonly EmailService _emailService;
    private readonly ILogger<AccessRequestRejectedConsumer> _logger;

    public AccessRequestRejectedConsumer(EmailService emailService, ILogger<AccessRequestRejectedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AccessRequestRejectedMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER] 📨 Solicitação de {Profile} recusada para {Email}",
            msg.RequestedProfile,
            msg.RecipientEmail);

        await _emailService.SendAccessRequestRejectedEmailAsync(
            msg.RecipientName,
            msg.RecipientEmail,
            msg.RequestedProfile);
    }
}
