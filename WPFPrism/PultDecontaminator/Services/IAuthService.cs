using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PultDecontominator.Services
{
    public interface IAuthService
    {
        Task<bool> Login(string UserName, string Password);
        Task<bool> CreateAccount(string UserName, string Password);
        Task<bool> LogOut();

    }
}
