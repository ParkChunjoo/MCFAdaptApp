using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCFAdaptApp.Application.Services
{
    public interface ILoginService
    {
        bool ValidateUser(string userId, string password);
    }
}
