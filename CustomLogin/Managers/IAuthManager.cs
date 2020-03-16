namespace CustomLogin.Managers
{
    public interface IAuthManager
    {
        User SignIn(string username, string password);
        User SignInAAD(string email);
    }
}
