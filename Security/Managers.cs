using latest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace latest.Security;

public class CareGiverManager : UserManager<CareGiver> {
    public CareGiverManager(
        IUserStore<CareGiver> store, 
        IOptions<IdentityOptions> optionsAccessor, 
        IPasswordHasher<CareGiver> passwordHasher, 
        IEnumerable<IUserValidator<CareGiver>> userValidators, 
        IEnumerable<IPasswordValidator<CareGiver>> passwordValidators, 
        ILookupNormalizer keyNormalizer, 
        IdentityErrorDescriber errors, 
        IServiceProvider services, 
        ILogger<UserManager<CareGiver>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) {
        RegisterTokenProvider(TokenOptions.DefaultProvider, new EmailTokenProvider<CareGiver>());
        // RegisterTokenProvider(TokenOptions.DefaultProvider, new );
        // RegisterTokenProvider<CareGiver>(TokenOptions.DefaultProvider, new EmailTokenProvider<CareGiver>());
    }

}
public class CareUserManager : UserManager<CareUser> {
    public CareUserManager(
        IUserStore<CareUser> store, 
        IOptions<IdentityOptions> optionsAccessor, 
        IPasswordHasher<CareUser> passwordHasher, 
        IEnumerable<IUserValidator<CareUser>> userValidators, 
        IEnumerable<IPasswordValidator<CareUser>> passwordValidators, 
        ILookupNormalizer keyNormalizer, 
        IdentityErrorDescriber errors, 
        IServiceProvider services, 
        ILogger<UserManager<CareUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) {
        // RegisterTokenProvider(TokenOptions.DefaultProvider, new EmailTokenProvider<CareUser>());
    }
}
