using System;
using System.Threading.Tasks;
using MCFAdaptApp.Domain.Services;
using MCFAdaptApp.Domain.Models;
using MCFAdaptApp.Infrastructure.Helpers;

namespace MCFAdaptApp.Infrastructure.Services
{
    /// <summary>
    /// Simple implementation of IAuthenticationService for demonstration purposes
    /// </summary>
    public class SimpleAuthenticationService : IAuthenticationService
    {
        private const string VALID_USER_ID = "SysAdmin";
        private const string VALID_PASSWORD = "SysAdmin";

        /// <summary>
        /// Authenticates a user with the provided credentials
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="password">The password</param>
        /// <returns>True if authentication is successful, false otherwise</returns>
        public Task<bool> AuthenticateAsync(string userId, string password)
        {
            LogHelper.Log($"Authentication attempt for user: {userId}");
            
            // Simple authentication logic for demonstration
            var isAuthenticated = userId == VALID_USER_ID && password == VALID_PASSWORD;
            
            LogHelper.Log($"Authentication result: {(isAuthenticated ? "Success" : "Failure")}");
            
            return Task.FromResult(isAuthenticated);
        }
    }
} 