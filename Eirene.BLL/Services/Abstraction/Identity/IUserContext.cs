namespace Eirene.BLL.Services.Abstraction.Identity
{
    public interface IUserContext
    {
        string? UserId { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
    }
}
