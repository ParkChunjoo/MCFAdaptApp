using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCFAdaptApp.Application.Services
{
    public class LoginService : ILoginService
    {
        public bool ValidateUser(string userId, string password)
        {
            // Simple check for "SysAdmin"
            return userId == "SysAdmin" && password == "SysAdmin";
        }
    }
}
