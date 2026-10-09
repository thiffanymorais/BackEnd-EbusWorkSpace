using FluentAssertions;
using MassTransit;
using Moq;
using JCA.WorkSpace.Application.Commands.AccessRequests;
using JCA.WorkSpace.Application.Handlers.AccessRequests;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Domain.Messages;

namespace JCA.WorkSpace.Application.Tests.Handlers.AccessRequests;

public class CreateAccessRequestHandlerTests
{
    private readonly Mock<IAccessRequestRepository> _accessRequestRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly CreateAccessRequestHandler _handler;

    public CreateAccessRequestHandlerTests()
    {
        _accessRequestRepoMock = new Mock<IAccessRequestRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _userRepoMock
            .Setup(x => x.GetActiveByProfileAsync(UserProfile.Admin))
            .ReturnsAsync(Array.Empty<User>());

        _handler = new CreateAccessRequestHandler(
            _accessRequestRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateAccessRequest_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateAccessRequestCommand { UserId = userId, RequestedProfile = UserProfile.Manager };
        
        var user = new User { Id = userId, Name = "Ana Aprendiz", Email = "ana.aprendiz@jcatlm.com.br", Profile = UserProfile.Employee };
        var admin = new User { Id = Guid.NewGuid(), Name = "Admin", Email = "admin@jcatlm.com.br", Profile = UserProfile.Admin, IsActive = true };
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepoMock.Setup(x => x.GetActiveByProfileAsync(UserProfile.Admin)).ReturnsAsync(new List<User> { admin });
        _accessRequestRepoMock.Setup(x => x.HasPendingRequestAsync(userId)).ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _accessRequestRepoMock.Verify(x => x.AddAsync(It.Is<AccessRequest>(a => a.UserId == userId && a.RequestedProfile == UserProfile.Manager)), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<AccessRequestPendingMessage>(m =>
                m.RequesterEmail == user.Email &&
                m.AdminEmail == admin.Email &&
                m.RequestedProfile == UserProfile.Manager),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserHasPendingRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateAccessRequestCommand { UserId = userId, RequestedProfile = UserProfile.Manager };
        
        var user = new User { Id = userId, Profile = UserProfile.Employee };
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _accessRequestRepoMock.Setup(x => x.HasPendingRequestAsync(userId)).ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Já existe uma solicitação de acesso pendente para este usuário.");
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<AccessRequestPendingMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
