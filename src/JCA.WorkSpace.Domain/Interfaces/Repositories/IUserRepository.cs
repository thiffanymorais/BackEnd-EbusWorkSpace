using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Domain.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task UpdateLastLoginAsync(Guid id);
    Task<IReadOnlyList<User>> GetActiveByProfileAsync(UserProfile profile);
}