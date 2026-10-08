using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Domain.Messages;

/// <summary>
/// Avisa cada administrador de que existe uma solicitação de perfil pendente.
/// O e-mail leva o administrador até a tela de aprovação. Não aprova sozinho.
/// </summary>
public record AccessRequestPendingMessage
{
    public Guid RequestId { get; init; }
    public string RequesterName { get; init; } = string.Empty;
    public string RequesterEmail { get; init; } = string.Empty;
    public UserProfile RequestedProfile { get; init; }
    public string AdminName { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Avisa quem pediu o perfil de que a solicitação foi aprovada e qual perfil passou a valer.
/// </summary>
public record AccessRequestApprovedMessage
{
    public Guid RequestId { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
    public UserProfile Profile { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Avisa quem pediu o perfil de que a solicitação foi recusada.
/// </summary>
public record AccessRequestRejectedMessage
{
    public Guid RequestId { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
    public UserProfile RequestedProfile { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
