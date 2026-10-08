using MediatR;
using MassTransit;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Domain.Messages;
using JCA.WorkSpace.Application.Commands.AccessRequests;

namespace JCA.WorkSpace.Application.Handlers.AccessRequests;

public class CreateAccessRequestHandler : IRequestHandler<CreateAccessRequestCommand, Guid>
{
    private readonly IAccessRequestRepository _accessRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateAccessRequestHandler(
        IAccessRequestRepository accessRequestRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _accessRequestRepository = accessRequestRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateAccessRequestCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null) throw new Exception("Usuário não encontrado.");
        if (user.Profile == request.RequestedProfile) throw new Exception("O usuário já possui este nível de acesso.");

        var hasPending = await _accessRequestRepository.HasPendingRequestAsync(request.UserId);
        if (hasPending) throw new Exception("Já existe uma solicitação de acesso pendente para este usuário.");

        var accessRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RequestedProfile = request.RequestedProfile,
            Status = AccessRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _accessRequestRepository.AddAsync(accessRequest);
        await _unitOfWork.CommitAsync();

        var admins = await _userRepository.GetActiveByProfileAsync(UserProfile.Admin);
        foreach (var admin in admins)
        {
            if (admin.Id == user.Id || string.IsNullOrWhiteSpace(admin.Email))
                continue;

            await _publishEndpoint.Publish(new AccessRequestPendingMessage
            {
                RequestId = accessRequest.Id,
                RequesterName = user.Name,
                RequesterEmail = user.Email,
                RequestedProfile = accessRequest.RequestedProfile,
                AdminName = admin.Name,
                AdminEmail = admin.Email
            }, cancellationToken);
        }

        return accessRequest.Id;
    }
}
