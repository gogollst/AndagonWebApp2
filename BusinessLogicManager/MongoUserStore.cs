using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using YourNamespace.Logic;
using AndagonWebApp2.Data;
using YourNamespace.Models;

namespace YourNamespace.Identity
{
    public class MongoUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserEmailStore<ApplicationUser>
    {
        private readonly BusinessLogicManager _manager;
        public MongoUserStore(BusinessLogicManager manager)
        {
            _manager = manager;
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Execute(async () =>
            {
                await _manager.CreateUserAsync(ToEntity(user));
                return IdentityResult.Success;
            });
        }

        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            // Deleting not supported yet
            return Task.FromResult(IdentityResult.Success);
        }

        public void Dispose() { }

        public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var entity = await _manager.GetUserByIdAsync(userId);
            return entity == null ? null : FromEntity(entity);
        }

        public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var entity = await _manager.GetUserByEmailAsync(normalizedUserName);
            return entity == null ? null : FromEntity(entity);
        }

        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
        public Task<string?> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());
        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Execute(async () =>
            {
                await _manager.UpdateUserAsync(ToEntity(user));
                return IdentityResult.Success;
            });
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash);
        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

        public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email);

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.EmailConfirmed);

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await _manager.GetUserByEmailAsync(normalizedEmail);
        }

        public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedEmail);

        public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        private static BusinessObjects.UserAccount ToEntity(ApplicationUser user)
        {
            return new BusinessObjects.UserAccount
            {
                Id = string.IsNullOrEmpty(user.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(user.Id),
                UserName = user.UserName,
                NormalizedUserName = user.NormalizedUserName,
                Email = user.Email,
                NormalizedEmail = user.NormalizedEmail,
                EmailConfirmed = user.EmailConfirmed,
                PasswordHash = user.PasswordHash,
                SecurityStamp = user.SecurityStamp,
                TwoFactorEnabled = user.TwoFactorEnabled
            };
        }

        private static ApplicationUser FromEntity(BusinessObjects.UserAccount entity)
        {
            return new ApplicationUser
            {
                Id = entity.Id.ToString(),
                UserName = entity.UserName,
                NormalizedUserName = entity.NormalizedUserName,
                Email = entity.Email,
                NormalizedEmail = entity.NormalizedEmail,
                EmailConfirmed = entity.EmailConfirmed,
                PasswordHash = entity.PasswordHash,
                SecurityStamp = entity.SecurityStamp,
                TwoFactorEnabled = entity.TwoFactorEnabled
            };
        }

        private static Task<IdentityResult> Execute(Func<Task> action)
        {
            return Execute(async () => { await action(); return IdentityResult.Success; });
        }

        private static async Task<IdentityResult> Execute(Func<Task<IdentityResult>> func)
        {
            try
            {
                return await func();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Database error." });
            }
        }
    }
}
