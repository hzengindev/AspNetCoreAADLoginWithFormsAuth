using System.Collections.Generic;
using System.Linq;

namespace CustomLogin.Managers
{
    public class AuthManager : IAuthManager
    {
        public static List<User> USERS = new List<User>
        {
            new User { Id = 1, Username = "user1", Password = "123", Fullname = "User 1", Email = "user1@company.com" },
            new User { Id = 2, Username = "user2", Password = "123", Fullname = "User 2", Email = "user2@company.com" },
        };

        public User SignIn(string username, string password)
        {
            var user = USERS.FirstOrDefault(z => z.Username == username && z.Password == z.Password);
            return user;
        }

        public User SignInAAD(string email)
        {
            var user = USERS.FirstOrDefault(z => z.Email == email);
            return user;
        }
    }
}