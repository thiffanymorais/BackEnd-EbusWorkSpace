using Microsoft.EntityFrameworkCore;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Infrastructure.Data.Contexts;

namespace JCA.WorkSpace.Infrastructure.Data.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(WorkSpaceContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

    public async Task UpdateLastLoginAsync(Guid id)
    {
        await _context.Users
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastLoginAt, DateTime.UtcNow));
    }

    public async Task<IReadOnlyList<User>> GetActiveByProfileAsync(UserProfile profile)
        => await _context.Users
            .Where(u => u.IsActive && u.Profile == profile)
            .ToListAsync();
}