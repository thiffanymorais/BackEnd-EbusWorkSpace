namespace JCA.WorkSpace.Domain.Messages;

/// <summary>
/// Mensagem (contrato) enviada para a fila do RabbitMQ
/// quando um novo usuário é criado no sistema.
/// O WelcomeEmailConsumer irá ler esta mensagem e disparar o e-mail.
/// </summary>
public record SendWelcomeEmailMessage
{
    public Guid UserId { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string? Sector { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
