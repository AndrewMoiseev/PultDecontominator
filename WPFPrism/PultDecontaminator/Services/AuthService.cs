using System.Threading.Tasks;

namespace PultDecontominator.Services
{
    public class AuthService : IAuthService
    {

        public async Task<bool> Login(string UserName, string Password)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> CreateAccount(string UserName, string Password)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> LogOut()
        {
            throw new System.NotImplementedException();
        }

    }
}