using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Infrastructure.Data.Repositories;
using JCA.WorkSpace.Infrastructure.Data.Contexts;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Infrastructure.Data.UnitOfWork;
using JCA.WorkSpace.Infrastructure.Data.Email;
using Microsoft.EntityFrameworkCore;

namespace JCA.WorkSpace.Infrastructure.CrossCutting.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WorkSpaceContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<WorkSpaceContextRead>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISpaceRepository, SpaceRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAccessRequestRepository, AccessRequestRepository>();
        services.AddScoped<IExtensionRequestRepository, ExtensionRequestRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Serviço de E-mail (MailKit)
        // Em desenvolvimento aponta para Mailpit (localhost:1025)
        // Em produção, alterar SmtpHost/SmtpPort no appsettings.json
        services.AddScoped<EmailService>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = AppDomain.CurrentDomain.Load("JCA.WorkSpace.Application");

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddAutoMapper(cfg => cfg.AddMaps(applicationAssembly));

        return services;
    }
}