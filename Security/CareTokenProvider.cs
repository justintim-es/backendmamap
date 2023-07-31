using latest.Models;
using Microsoft.AspNetCore.Identity;

namespace latest.Security;

// public class CareTokenProvider<TUser> : IUserTwoFactorTokenProvider<TUser> where TUser : Care {
//         public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
//         {
//             if (manager != null && user != null)
//             {
//                 return Task.FromResult(true);
//             }
//             else
//             {
//                 return Task.FromResult(false);
//             }
//         }

//         // Genereates a simple token based on the user id, email and another string.
//         private string GenerateToken(Care user, string purpose)
//         {
//             string secretString = "coffeIsGood";
//             return secretString + user.Email + purpose + user.Id;
//         }

//         public Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
//         {
//             return Task.FromResult(GenerateToken(user, purpose));
//         }

//         public Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
//         {
//             return Task.FromResult(token == GenerateToken(user, purpose));
//         }
// }