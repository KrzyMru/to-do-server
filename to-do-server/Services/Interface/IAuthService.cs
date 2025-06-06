namespace to_do_server.Services.Interface
{
    public interface IAuthService
    {
        string GenerateToken(int userId);
        bool ValidateToken(string token);
        string GetUserId(string token);
    }
}
