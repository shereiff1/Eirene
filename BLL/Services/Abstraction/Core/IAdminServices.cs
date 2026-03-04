using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Models.Core.Admin;

namespace BLL.Services.Abstraction.Core
{
    public interface IAdminServices
    {
        Task<(bool IsSuccess, List<AdminModel>? Admins)> GetAllAsync();
        Task<(bool IsSuccess, AdminModel? Admin)> GetByIdAsync(string adminId);
        Task<(bool IsSuccess, string? Error, AdminModel? Admin)> CreateAdminProfileAsync(string userId);
        Task<bool> AssignRoleAsync(string adminId, string userId, string role);
        Task<bool> ManageCommunityGroupMembershipAsync(Guid groupId, string userId, bool assign);
    }
}
