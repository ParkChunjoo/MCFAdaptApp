using System.Threading.Tasks;

namespace MCFAdaptApp.Domain.Services
{
    /// <summary>
    /// Interface for authentication services
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates a user with the provided credentials
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="password">The password</param>
        /// <returns>True if authentication is successful, false otherwise</returns>
        Task<bool> AuthenticateAsync(string userId, string password);
    }
}