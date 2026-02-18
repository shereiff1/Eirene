
namespace BLL.Enumerators;

public static class Roles
{
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    public const string Moderator = "Moderator";
    public const string Admin = "Admin";
    public const string AdminOrModerator = "Admin,Moderator";
    public const string AllUsers = "Patient,Doctor,Moderator,Admin";
    public const string DoctorOrAdmin = "Doctor,Admin";
    public const string AllExceptDoctor = "Patient,Moderator,Admin";
}
