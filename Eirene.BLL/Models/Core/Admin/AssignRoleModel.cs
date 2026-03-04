using System.ComponentModel.DataAnnotations;
using BLL.Enumerators;

namespace BLL.Models.Core.Admin;

public class AssignRoleModel
{
    [Required]
    public string userId {get; set;} = string.Empty;
    [Required]
    [AllowedValues(Roles.Moderator, Roles.Doctor, Roles.Patient)]
    public string role {get; set;} = Roles.Patient;
}