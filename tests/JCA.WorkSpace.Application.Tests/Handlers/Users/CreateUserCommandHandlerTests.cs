using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Users;
using JCA.WorkSpace.Application.Handlers.Users;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using MassTransit;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Users;

[Collection(nameof(UserFixtureCollection))]
public class CreateUserCommandHandlerTests
{
    private readonly UserTestsFixture _fixture;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests(UserTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();

        _handler = new CreateUserCommandHandler(
            _fixture.UserRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object,
            _fixture.PublishEndpointMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateUserAndReturnGuid_WhenEmailIsUnique()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "Carlos Silva",
            Email = "carlos.silva@jcatlm.com",
            Sector = "Financeiro",
            Profile = UserProfile.Manager
        };

        _fixture.UserRepositoryMock
            .Setup(repo => repo.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _fixture.UserRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u => 
            u.Name == command.Name &&
            u.Email == command.Email &&
            u.Sector == command.Sector &&
            u.Profile == command.Profile &&
            u.IsActive == true
        )), Times.Once);

        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowExceptionAndNotCommit_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "Carlos Silva",
            Email = "carlos.silva@jcatlm.com",
            Sector = "Financeiro",
            Profile = UserProfile.Manager
        };

        var existingUser = _fixture.GenerateValidUser();

        _fixture.UserRepositoryMock
            .Setup(repo => repo.GetByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("J* existe um usu*rio cadastrado com este e-mail.");

        _fixture.UserRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Never);
    }
}
