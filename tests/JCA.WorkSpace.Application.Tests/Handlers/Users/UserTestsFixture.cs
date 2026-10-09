using AutoMapper;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Users;

public class UserTestsFixture
{
    public Mock<IUserRepository> UserRepositoryMock { get; set; }
    public Mock<IAuditLogRepository> AuditLogRepositoryMock { get; set; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; set; }
    public Mock<IConfiguration> ConfigurationMock { get; set; }
    public Mock<IMapper> MapperMock { get; set; }
    public Mock<IPublishEndpoint> PublishEndpointMock { get; set; }

    public UserTestsFixture()
    {
        UserRepositoryMock = new Mock<IUserRepository>();
        AuditLogRepositoryMock = new Mock<IAuditLogRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        ConfigurationMock = new Mock<IConfiguration>();
        MapperMock = new Mock<IMapper>();
        PublishEndpointMock = new Mock<IPublishEndpoint>();
    }

    public void ResetMocks()
    {
        UserRepositoryMock.Reset();
        AuditLogRepositoryMock.Reset();
        UnitOfWorkMock.Reset();
        ConfigurationMock.Reset();
        MapperMock.Reset();
        PublishEndpointMock.Reset();

        UnitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);

        var jwtSettings = new Dictionary<string, string>
        {
            {"JwtSettings:Secret", "CHAVE_SUPER_SECRETA_MUITO_LONGA_PARA_O_TESTE_32_BYTES"},
            {"JwtSettings:ExpirationInMinutes", "60"},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"GoogleAuth:ClientId", "test-client-id.apps.googleusercontent.com"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(jwtSettings!)
            .Build();

        ConfigurationMock.Setup(c => c.GetSection(It.IsAny<string>()))
            .Returns((string key) => configuration.GetSection(key));
            
        ConfigurationMock.Setup(c => c[It.IsAny<string>()])
            .Returns((string key) => configuration[key]);
    }

    public User GenerateValidUser(UserProfile profile = UserProfile.Employee)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = "João da Silva",
            Email = "joao.silva@jcatlm.com",
            Profile = profile,
            Sector = "Tecnologia",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };
    }
}

[CollectionDefinition(nameof(UserFixtureCollection))]
public class UserFixtureCollection : ICollectionFixture<UserTestsFixture>
{
}
