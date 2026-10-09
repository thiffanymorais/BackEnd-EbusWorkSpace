using MediatR;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.Users;
using JCA.WorkSpace.Domain.Messages;
using MassTransit;

namespace JCA.WorkSpace.Application.Handlers.Users;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateUserCommandHandler(
        IUserRepository userRepository, 
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Já existe um usuário cadastrado com este e-mail.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Sector = request.Sector,
            Profile = request.Profile,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.CommitAsync();

        // Publica o evento na fila do RabbitMQ
        // O WelcomeEmailConsumer vai pegar esta mensagem e enviar o e-mail de boas-vindas
        await _publishEndpoint.Publish(new SendWelcomeEmailMessage
        {
            UserId = user.Id,
            RecipientName = user.Name,
            RecipientEmail = user.Email,
            Role = user.Profile.ToString(),
            Sector = user.Sector
        });

        return user.Id;
    }
}