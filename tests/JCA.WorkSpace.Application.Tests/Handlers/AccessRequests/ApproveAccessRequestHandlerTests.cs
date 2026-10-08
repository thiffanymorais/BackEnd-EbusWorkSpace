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

public class ApproveAccessRequestHandlerTests
{
    private readonly Mock<IAccessRequestRepository> _accessRequestRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly ApproveAccessRequestHandler _handler;

    public ApproveAccessRequestHandlerTests()
    {
        _accessRequestRepoMock = new Mock<IAccessRequestRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _handler = new ApproveAccessRequestHandler(
            _accessRequestRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldApproveRequestAndUpdateUser_WhenValid()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new ApproveAccessRequestCommand { RequestId = requestId };
        
        var accessRequest = new AccessRequest 
        { 
            Id = requestId, 
            UserId = userId, 
            RequestedProfile = UserProfile.Admin, 
            Status = AccessRequestStatus.Pending 
        };
        
        var user = new User { Id = userId, Name = "Ana Aprendiz", Email = "ana.aprendiz@jcatlm.com.br", Profile = UserProfile.Employee };
        
        _accessRequestRepoMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync(accessRequest);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        accessRequest.Status.Should().Be(AccessRequestStatus.Approved);
        user.Profile.Should().Be(UserProfile.Admin);
        
        _accessRequestRepoMock.Verify(x => x.UpdateAsync(accessRequest), Times.Once);
        _userRepoMock.Verify(x => x.UpdateAsync(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<AccessRequestApprovedMessage>(m =>
                m.RecipientEmail == user.Email &&
                m.Profile == UserProfile.Admin),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRequestNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new ApproveAccessRequestCommand { RequestId = requestId };
        
        _accessRequestRepoMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync((AccessRequest)null!);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Solicitação de acesso não encontrada.");
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<AccessRequestApprovedMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
