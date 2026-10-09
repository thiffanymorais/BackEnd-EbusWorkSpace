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

public class RejectAccessRequestHandlerTests
{
    private readonly Mock<IAccessRequestRepository> _accessRequestRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly RejectAccessRequestHandler _handler;

    public RejectAccessRequestHandlerTests()
    {
        _accessRequestRepoMock = new Mock<IAccessRequestRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _handler = new RejectAccessRequestHandler(
            _accessRequestRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRejectRequest_WhenValid()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RejectAccessRequestCommand { RequestId = requestId };
        
        var accessRequest = new AccessRequest 
        { 
            Id = requestId,
            UserId = userId,
            RequestedProfile = UserProfile.Facilities,
            Status = AccessRequestStatus.Pending 
        };
        var user = new User { Id = userId, Name = "Ana Aprendiz", Email = "ana.aprendiz@jcatlm.com.br", Profile = UserProfile.Employee };
        
        _accessRequestRepoMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync(accessRequest);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        accessRequest.Status.Should().Be(AccessRequestStatus.Rejected);
        
        _accessRequestRepoMock.Verify(x => x.UpdateAsync(accessRequest), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<AccessRequestRejectedMessage>(m =>
                m.RecipientEmail == user.Email &&
                m.RequestedProfile == UserProfile.Facilities),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRequestNotPending()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new RejectAccessRequestCommand { RequestId = requestId };
        
        var accessRequest = new AccessRequest 
        { 
            Id = requestId, 
            Status = AccessRequestStatus.Approved // Já aprovada
        };
        
        _accessRequestRepoMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync(accessRequest);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("A solicitação não está pendente.");
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<AccessRequestRejectedMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
